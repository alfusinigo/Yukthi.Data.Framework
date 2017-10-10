using System;
using System.Collections.Generic;
using System.Text;

namespace Yc.Sql.Entity.Data.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.GenericParameter)]
    public class IsInterface : ConstraintAttribute
    {
        public override bool Check(Type genericType)
        {
            return genericType.IsInterface;
        }

        public override string ToString()
        {
            return "Generic type is not an interface";
        }
    }

    public abstract class ConstraintAttribute : Attribute
    {
        public ConstraintAttribute() { }

        public abstract bool Check(Type generic);
    }
}
