using System;
using System.Collections.Generic;
using System.Data;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Context
{
    public interface IBaseContext
    {
        string Command { get; set; }
        CommandType CommandType { get; set; }
        int Timeout { get; set; }
        string ControllerFunction { get; set; }
        string DependingDbTableNamesInCsv { get; set; }
        bool IsLongRunning { get; set; }
        bool MustCache { get; set; }
        Dictionary<string, KeyValuePair<object, Type>> DbParameterContainer { get; }
        
        void SetEntityValue(string entityName, object value);
    }
}
