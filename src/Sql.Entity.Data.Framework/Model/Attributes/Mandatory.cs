using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MandatoryAttribute : Attribute
    {
        private string descriptiveAttributeName;
        private string[] functions;

        public MandatoryAttribute(params string[] functions)
            : this(null, functions)
        {
            
        }
        public MandatoryAttribute(string descriptiveAttributeName, params string[] functions)
        {
            this.descriptiveAttributeName = descriptiveAttributeName;
            this.functions = functions;
        }

        public string DescriptiveAttributeName
        {
            get { return descriptiveAttributeName; }
        }

        public string[] Functions
        {
            get { return functions; }
        }
    }
}
