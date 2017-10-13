using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Microsoft.Extensions.Logging;
using Yc.Sql.Entity.Data.Core.Framework.Helper;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Yc.Sql.Entity.Data.Core.Framework.Mapper
{
    public abstract class DataMapperBase : IDataMapper
    {
        private IDatabase database;
        private ICacheRepository cacheRepository;
        private ILogger<DataMapperBase> logger;
        private HashSet<string> unsupportedTypes = new HashSet<string>();

        protected DataMapperBase(IDatabase database, ICacheRepository cacheRepository, ILogger<DataMapperBase> logger)
        {
            this.database = database;
            this.cacheRepository = cacheRepository;
            this.logger = logger;

            if (cacheRepository == null)
                logger.LogWarning("Data caching is disabled, no cache service added!");
        }

        protected DataMapperBase(IDatabase database, ILogger<DataMapperBase> logger)
            : this(database, null, logger)
        {
        }

        public List<T> GetDataItems<T>(IBaseContext context)
        {
            SetFunctionSpecificEntityMappings(context);
            return
                (List<T>)
                    CheckSetAndReturnValue<T>(MapperBase_OnCallForGetEntities<T>, context, DeserialzeEntities<T>);
        }

        private object MapperBase_OnCallForGetEntities<T>(IBaseContext context, List<IDataParameter> parameterCollection)
        {
            var reader = GetReader(context);
            return BuildEntityList<T>(reader);
        }

        private object MapperBase_OnCallForGetEntity<T>(IBaseContext context, List<IDataParameter> parameterCollection)
        {
            var reader = GetReader(context);
            object data = null;

            while (reader.Read())
            {
                data = BuildEntity<T>(reader);
            }
            return data;
        }

        public T GetDataItem<T>(IBaseContext context)
        {
            SetFunctionSpecificEntityMappings(context);
            return
                (T)
                    CheckSetAndReturnValue<T>(MapperBase_OnCallForGetEntity<T>, context, DeserialzeEntity<T>);
        }

        public IDataReader GetReader(IBaseContext context)
        {
            SetFunctionSpecificEntityMappings(context);
            switch (context.CommandType)
            {
                case CommandType.StoredProcedure:
                    return database.ExecuteReaderStoredProcedure(context.Command,
                        context.Timeout, BuildParameters(context));
                case CommandType.Text:
                    return database.ExecuteReader(context.Command,
                        context.Timeout, BuildParameters(context));
                default:
                    throw new NotImplementedException($"CommandType {Enum.GetName(typeof(CommandType),context.CommandType)} is not supported");
            }
        }

        public dynamic SubmitData(IBaseContext context)
        {
            SetFunctionSpecificEntityMappings(context);
            return database.ExecuteScalar(context.CommandType, context.Command, context.Timeout, BuildParameters(context));
        }

        public void SubmitData(IEnumerable<IBaseContext> entities)
        {
            foreach (var context in entities)
            {
                SubmitData(context);
            }
        }

        public abstract void SetFunctionSpecificEntityMappings(IBaseContext context);

        private dynamic CheckSetAndReturnValue<T>(DatabaseMethod<T> databaseMethod, IBaseContext context, DeserializeMethod<T> deserializeMethod)
        {
            var parameters = BuildParameters(context);
            if (cacheRepository == null || (context.IsLongRunning && !context.MustCache))
            {
                LogDataRetrievalInfo(databaseMethod, context, false);
                return databaseMethod(context, parameters.ToList());
            }

            var typeFullName = typeof(T).FullName;

            if (!cacheRepository.ContainsValue($"{context.Command}-{typeFullName}", context.DependingDbTableNamesInCsv, parameters))
            {
                var data = databaseMethod(context, parameters.ToList());
                cacheRepository[$"{context.Command}-{typeFullName}", context.DependingDbTableNamesInCsv, parameters] = JsonConvert.SerializeObject(data);

                LogDataRetrievalInfo(databaseMethod, context, false);
                return deserializeMethod(cacheRepository[$"{context.Command}-{typeFullName}", context.DependingDbTableNamesInCsv, parameters]);
            }

            LogDataRetrievalInfo(databaseMethod, context, true);
            return deserializeMethod(cacheRepository[$"{context.Command}-{typeFullName}", context.DependingDbTableNamesInCsv, parameters]);
        }

        private object DeserialzeEntity<T>(string jsonContent)
        {
            return JsonConvert.DeserializeObject<T>(jsonContent);
        }

        private object DeserialzeEntities<T>(string jsonContent)
        {
            return JsonConvert.DeserializeObject<List<T>>(jsonContent);
        }

        private void LogDataRetrievalInfo<T>(DatabaseMethod<T> databaseMethod, IBaseContext context, bool isFromCache)
        {
            logger.LogDebug($"Mapper-Data returned from {(isFromCache ? "Cache" : "Database")}, Mapper: {GetType().Name}, Context: {context.GetType().Name}, Function: {context.ControllerFunction}");
        }

        private IDataParameter[] BuildParameters(IBaseContext context)
        {
            return context.DbParameterContainer.Select(data => new SqlParameter(data.Key, data.Value.Key)).ToArray();
        }

        private IEnumerable<T> BuildEntityList<T>(IDataReader reader)
        {
            var entityList = new List<T>();
            while (reader.Read())
            {
                entityList.Add(BuildEntity<T>(reader));
            }

            return entityList;
        }

        private T BuildEntity<T>(IDataReader reader)
        {
            var dynamicEntity = new ExpandoObject();
            IBaseContext regularEntity = null;
            bool isBaseContextType = false;

            var typeName = typeof(T).FullName;

            if (!unsupportedTypes.Contains(typeName))
            {
                try
                {
                    regularEntity = (IBaseContext)Activator.CreateInstance(typeof(T));
                    isBaseContextType = true;
                }
                catch (InvalidCastException)
                {
                    unsupportedTypes.Add(typeName);
                    isBaseContextType = false;
                }
                catch (Exception exception)
                {
                    throw new NotSupportedException($"Type<T> {typeof(T).FullName} is not supported, either dynamic or IBaseContext implementations are only supported!", exception);
                }
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.GetValue(i);
                var type = reader.GetFieldType(i);

                if (Convert.IsDBNull(value))
                    continue;

                AddProperty(dynamicEntity, columnName, value, type);

                if (isBaseContextType)
                    regularEntity.SetEntityValue(columnName, value);
            }

            try
            {
                if (!isBaseContextType)
                    return (dynamic)dynamicEntity;
            }
            catch (RuntimeBinderException exception)
            {
                throw new NotSupportedException($"Type<T> {typeof(T).FullName} is not supported, either dynamic or IBaseContext implementations are only supported!", exception);
            }

            return (T)regularEntity;
        }

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue, Type propertyType)
        {
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
