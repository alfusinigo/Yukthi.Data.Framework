using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FunctionsAttribute : Attribute
    {
        private string[] functions;
        public FunctionsAttribute(params string[] functions)
        {
            this.functions = functions;
        }

        public string[] Functions
        {
            get { return functions; }
        }
    }
}
