using System;
using System.ComponentModel;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    public enum Status
    {
        [Description("Success")]
        Success,

        [Description("Failure")]
        Failure
    }
}
