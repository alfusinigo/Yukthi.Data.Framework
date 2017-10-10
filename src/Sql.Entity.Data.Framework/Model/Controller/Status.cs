using System;
using System.ComponentModel;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    [Serializable]
    public enum Status
    {
        [Description("Success")]
        Success,

        [Description("Failure")]
        Failure
    }
}
