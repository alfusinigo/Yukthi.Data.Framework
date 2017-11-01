using Yc.Sql.Entity.Data.Core.Framework.Mapper;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Linq;
using Yc.Sql.Entity.Data.Core.Framework.Helper;

namespace Yc.Sql.Entity.Data.Core.Framework.Controller
{
    public abstract class BaseDataController : IDataController
    {
        protected IDataMapper dataMapper;
        protected IConcurrentProcessor concurrentProcessor;
        private ILogger<BaseDataController> logger;

        protected BaseDataController(IDataMapper dataMapper, IConcurrentProcessor concurrentProcessor, ILogger<BaseDataController> logger)
            : this(dataMapper, logger)
        {
            this.concurrentProcessor = concurrentProcessor;
        }

        protected BaseDataController(IDataMapper dataMapper, ILogger<BaseDataController> logger)
        {
            this.dataMapper = dataMapper;
            this.logger = logger;
        }

        public IDataResponseInfo SubmitChanges<T>(T entity) where T : IBaseContext
        {
            return SubmitChanges(entity, new CorrelationInfo());
        }

        public IDataResponseInfo SubmitChanges<T>(List<T> entities) where T : IBaseContext
        {
            return SubmitChanges(entities, new CorrelationInfo());
        }

        public IDataResponseInfo GetEntity<T>(IBaseContext entity)
        {
            return GetEntity<T>(entity, new CorrelationInfo());
        }

        public IDataResponseInfo GetEntities<T>(IBaseContext entity)
        {
            return GetEntities<T>(entity, new CorrelationInfo());
        }

        public virtual IDataResponseInfo SubmitChanges<T>(T entity, ICorrelationInfo correlationInfo) where T : IBaseContext
        {
            var responseInfo = new DataResponseInfo();
            try
            {
                OnStart(this, dataMapper, entity, correlationInfo, responseInfo);
                responseInfo.Data = dataMapper.SubmitData(entity);
                OnCompletion(this, dataMapper, entity, correlationInfo);
            }
            catch (Exception exception)
            {
                OnException(this, dataMapper, entity, correlationInfo, responseInfo, exception);
            }
            return responseInfo;
        }

        public virtual IDataResponseInfo SubmitChanges<T>(List<T> entities, ICorrelationInfo correlationInfo) where T : IBaseContext
        {
            var responseInfo = new DataResponseInfo();
            try
            {
                OnStart(this, dataMapper, entities[0], correlationInfo, responseInfo);
                var castedData = new List<IBaseContext>();
                if (entities != null)
                    castedData.AddRange(entities.Cast<IBaseContext>());
                dataMapper.SubmitData(castedData);
                OnCompletion(this, dataMapper, entities[0], correlationInfo);
                responseInfo.Data = true;
            }
            catch (Exception exception)
            {
                OnException(this, dataMapper, entities[0], correlationInfo, responseInfo, exception);
            }
            return responseInfo;
        }

        public virtual IDataResponseInfo GetEntity<T>(IBaseContext entity, ICorrelationInfo correlationInfo)
        {
            var responseInfo = new DataResponseInfo();
            try
            {
                OnStart(this, dataMapper, entity, correlationInfo, responseInfo);
                responseInfo.Data = dataMapper.GetDataItem<T>(entity);
                OnCompletion(this, dataMapper, entity, correlationInfo);
            }
            catch (Exception exception)
            {
                OnException(this, dataMapper, entity, correlationInfo, responseInfo, exception);
            }
            return responseInfo;
        }

        public IDataResponseInfo GetEntities<T>(IBaseContext entity, ICorrelationInfo correlationInfo)
        {
            var responseInfo = new DataResponseInfo();
            try
            {
                OnStart(this, dataMapper, entity, correlationInfo, responseInfo);
                responseInfo.Data = GetCastedCollection<T>(dataMapper.GetDataItems<T>(entity));
                OnCompletion(this, dataMapper, entity, correlationInfo);
            }
            catch (Exception exception)
            {
                OnException(this, dataMapper, entity, correlationInfo, responseInfo, exception);
            }
            return responseInfo;
        }

        private IEnumerable<T> GetCastedCollection<T>(IEnumerable<T> uncastedData)
        {
            var data = uncastedData.ToList();
            var castedData = new List<T>();
            if (data != null)
                castedData.AddRange(data.Cast<T>());
            return castedData;
        }

        protected internal virtual void OnCustomMessage(IDataController controller, IDataMapper mapper, IBaseContext context, ICorrelationInfo correlationInfo, string message)
        {
            logger.LogDebug($"Message: DataController: {controller.GetType().Name}, DataMapper: {mapper.GetType().Name}, Context: {context?.GetType().Name}, CorrelationId: {correlationInfo?.CorrelationId}, Function: {context?.ControllerFunction}, User: {correlationInfo?.RequestorName}\n{message}");
        }

        protected internal virtual void OnException(IDataController controller, IDataMapper mapper, IBaseContext context, ICorrelationInfo correlationInfo, IDataResponseInfo response, Exception exception)
        {
            response.Status = Status.Failure;

            if (exception is NoNullAllowedException)
            {
                response.Message = response.Message.Insert(0, exception.Message);
            }
            else
            {
                response.Message = response.Message.Insert(0, exception.ToString());
                logger.LogError($"Exception: DataController: {controller.GetType().Name}, DataMapper: {mapper.GetType().Name}, Context: {context?.GetType().Name}, Function: {context?.ControllerFunction}, CorrelationId: {correlationInfo?.CorrelationId}, User: {correlationInfo?.RequestorName}\n{exception}", exception);
            }
        }

        protected internal virtual void OnStart(IDataController controller, IDataMapper mapper, IBaseContext context, ICorrelationInfo correlationInfo, IDataResponseInfo response)
        {
            InitializeResponse(response, correlationInfo);
            logger.LogDebug($"Started: DataController: {controller.GetType().Name}, DataMapper: {mapper.GetType().Name}, Context: {context?.GetType().Name}, CorrelationId: {correlationInfo?.CorrelationId}, Function: {context?.ControllerFunction}, User: {correlationInfo?.RequestorName}");
        }

        protected internal virtual void OnCompletion(IDataController controller, IDataMapper mapper, IBaseContext context, ICorrelationInfo correlationInfo)
        {
            logger.LogDebug($"Completed: DataController: {controller.GetType().Name}, DataMapper: {mapper.GetType().Name}, Context: {context?.GetType().Name}, CorrelationId: {correlationInfo?.CorrelationId}, Function: {context?.ControllerFunction}, User: {correlationInfo?.RequestorName}");
        }

        protected internal virtual bool VerifyResponseStatus(IDataResponseInfo response, bool throwException)
        {
            if (response.Status == Status.Failure)
            {
                if (throwException)
                    throw new Exception(response.Message);
                return false;
            }
            return true;
        }

        protected internal virtual IDataResponseInfo InitializeResponse(IDataResponseInfo response, ICorrelationInfo correlationInfo)
        {
            if (response == null)
                response = new DataResponseInfo();

            response.CorrelationId = correlationInfo?.CorrelationId;
            response.HostName = Environment.MachineName;
            return response;
        }
    }
}
