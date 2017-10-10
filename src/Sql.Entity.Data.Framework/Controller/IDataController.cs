using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;
using System;
using System.Collections.Generic;

namespace Yc.Sql.Entity.Data.Core.Framework.Controller
{
    public interface IDataController
    {
        IDataResponseInfo SubmitChanges<T>(T entity, IDataRequestInfo requestInfo) where T : IBaseContext;

        IDataResponseInfo SubmitChanges<T>(List<T> entities, IDataRequestInfo requestInfo) where T : IBaseContext;

        IDataResponseInfo GetEntity<T>(T entity, IDataRequestInfo requestInfo) where T : IBaseContext;

        IDataResponseInfo GetEntities<T>(T entity, IDataRequestInfo requestInfo) where T : IBaseContext;
    }
}
