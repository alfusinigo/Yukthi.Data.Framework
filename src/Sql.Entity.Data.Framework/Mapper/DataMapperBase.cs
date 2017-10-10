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

namespace Yc.Sql.Entity.Data.Core.Framework.Mapper
{
    public abstract class DataMapperBase : IDataMapper
    {
        private IDatabase database;
        private ICacheRepository cacheRepository;
        private ILogger<DataMapperBase> logger;
        IBinarySerializer serializer;

        public event DatabaseMethod OnCallForGetEntity;
        public event DatabaseMethod OnCallForGetEntities;

        protected DataMapperBase(IDatabase database, ICacheRepository cacheRepository, ILogger<DataMapperBase> logger)
        {
            this.database = database;
            this.cacheRepository = cacheRepository;
            this.logger = logger;

            serializer = new BinarySerializer();

            OnCallForGetEntity += MapperBase_OnCallForGetEntity;
            OnCallForGetEntities += MapperBase_OnCallForGetEntities;
        }

        protected DataMapperBase(IDatabase database, ILogger<DataMapperBase> logger)
            : this(database, null, logger)
        {
        }

        object MapperBase_OnCallForGetEntities(IBaseContext context, List<IDataParameter> parameterCollection, Type returnEntityType)
        {
            var reader = GetReader(context);
            return BuildEntityList(reader, returnEntityType);
        }

        object MapperBase_OnCallForGetEntity(IBaseContext context, List<IDataParameter> parameterCollection, Type returnEntityType)
        {
            var reader = GetReader(context);
            while (reader.Read())
            {
                return BuildEntity(reader, returnEntityType);
            }
            return null;
        }

        public IEnumerable<IBaseContext> GetDataItems(IBaseContext context, Type returnEntityType)
        {
            SetFunctionSpecificEntityMappings(context);
            return
                (IEnumerable<IBaseContext>)
                    CheckSetAndReturnValue(MapperBase_OnCallForGetEntities, context, returnEntityType);
        }

        public IBaseContext GetDataItem(IBaseContext context, Type returnEntityType)
        {
            SetFunctionSpecificEntityMappings(context);
            return
                (IBaseContext)
                    CheckSetAndReturnValue(MapperBase_OnCallForGetEntity, context, returnEntityType);
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
                    throw new NotImplementedException(context.CommandType.ToString());
            }
        }

        public object SubmitData(IBaseContext context)
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

        private object CheckSetAndReturnValue(DatabaseMethod databaseMethod, IBaseContext context, Type returnEntityType)
        {
            var parameters = BuildParameters(context);
            if (cacheRepository == null || (context.IsLongRunning && !context.MustCache))
            {
                LogDataRetrievalInfo(databaseMethod, context, false);
                return databaseMethod(context, parameters.ToList(), returnEntityType);
            }

            if (!cacheRepository.ContainsValue(context.Command, context.DependingDbTableNamesInCsv, parameters))
            {
                if (cacheRepository.ContainsValue(context.Command, context.DependingDbTableNamesInCsv, parameters))
                {
                    LogDataRetrievalInfo(databaseMethod, context, true);
                    return serializer.ConvertByteArrayToObject(cacheRepository[context.Command, context.DependingDbTableNamesInCsv, parameters]);
                }

                var callMethod = databaseMethod;
                var paramCollection = new List<IDataParameter>(parameters);
                var currentContext = context.Clone();
                var returnType = returnEntityType;
                cacheRepository[context.Command, context.DependingDbTableNamesInCsv, parameters] = serializer.ConvertObjectToByteArray(callMethod(currentContext, paramCollection, returnType));

                LogDataRetrievalInfo(databaseMethod, context, false);
                return serializer.ConvertByteArrayToObject(cacheRepository[context.Command, context.DependingDbTableNamesInCsv, parameters]);
            }
            LogDataRetrievalInfo(databaseMethod, context, true);
            return serializer.ConvertByteArrayToObject(cacheRepository[context.Command, context.DependingDbTableNamesInCsv, parameters]);
        }

        private void LogDataRetrievalInfo(DatabaseMethod databaseMethod, IBaseContext context, bool isFromCache)
        {
            logger.LogDebug($"Mapper-Data returned from {(isFromCache ? "Cache" : "Database")}, Mapper: {GetType().Name}, Context: {context.GetType().Name}, Function: {context.ControllerFunction}");
        }

        private IDataParameter[] BuildParameters(IBaseContext context)
        {
            return context.DbParameterContainer.Select(data => new SqlParameter(data.Key, data.Value.Key)).ToArray();
        }

        private IEnumerable<IBaseContext> BuildEntityList(IDataReader reader, Type entityType)
        {
            var entityList = new List<IBaseContext>();
            while (reader.Read())
            {
                entityList.Add(BuildEntity(reader, entityType));
            }
            return entityList;
        }

        private IBaseContext BuildEntity(IDataReader reader, Type entityType)
        {
            var entity = (IBaseContext)Activator.CreateInstance(entityType);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.GetValue(i);

                if (Convert.IsDBNull(value))
                    continue;

                entity.SetEntityValue(columnName, value);
            }
            return entity;
        }
    }
}
