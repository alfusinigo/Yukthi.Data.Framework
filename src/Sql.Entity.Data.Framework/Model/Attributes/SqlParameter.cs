using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlParameterAttribute : Attribute
    {
        private string parameterName;
        public SqlParameterAttribute(string parameterName)
        {
            this.parameterName = parameterName;
        }

        public string ParameterName
        {
            get { return parameterName; }
        }
    }
}
