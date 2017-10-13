using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    public interface IDataRequestInfo
    {
        string RequestorName { get; set; }
        string CorrelationId { get; set; }
    }

    public class DataRequestInfo : IDataRequestInfo
    {
        public string RequestorName { get; set; }
        public string CorrelationId { get; set; }
    }
}
