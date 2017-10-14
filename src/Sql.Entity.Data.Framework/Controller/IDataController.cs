using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;
using System;
using System.Collections.Generic;

namespace Yc.Sql.Entity.Data.Core.Framework.Controller
{
    public interface IDataController
    {
        IDataResponseInfo SubmitChanges<T>(T entity, ICorrelationInfo requestInfo) where T : IBaseContext;

        IDataResponseInfo SubmitChanges<T>(List<T> entities, ICorrelationInfo requestInfo) where T : IBaseContext;

        IDataResponseInfo GetEntity<T>(IBaseContext entity, ICorrelationInfo requestInfo);

        IDataResponseInfo GetEntities<T>(IBaseContext entity, ICorrelationInfo requestInfo);
    }
}
