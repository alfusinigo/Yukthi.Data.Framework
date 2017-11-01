using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    public interface ICorrelationInfo
    {
        string RequestorName { get; set; }
        string CorrelationId { get; set; }
    }

    public class CorrelationInfo : ICorrelationInfo
    {
        public string RequestorName { get; set; } = "N/A";
        public string CorrelationId { get; set; } = "N/A";
    }
}
