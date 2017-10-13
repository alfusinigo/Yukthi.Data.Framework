using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    public interface IDataResponseInfo
    {
        dynamic Data { get; set; }
        string HostName { get; set; }
        string Message { get; set; }
        Status Status { get; set; }
        string CorrelationId { get; set; }
    }

    public class DataResponseInfo : IDataResponseInfo
    {
        private string message = string.Empty;
        public dynamic Data { get; set; }
        public string HostName { get; set; }
        public Status Status { get; set; }
        public string CorrelationId { get; set; }
        public string Message 
        {
            get { return message; }
            set { message = value; }
        }
    }
}
