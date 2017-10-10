using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    public interface IDataResponseInfo
    {
        object Data { get; set; }
        string HostName { get; set; }
        string Message { get; set; }
        Status Status { get; set; }
        string CorrelationId { get; set; }
    }

    [Serializable]
    public class DataResponseInfo : IDataResponseInfo
    {
        private string message = string.Empty;
        public object Data { get; set; }
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
