using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNamesAttribute : Attribute
    {
        private string[] columnNames;
        public ColumnNamesAttribute(params string[] columnNames)
        {
            this.columnNames = columnNames;
        }

        public string[] ColumnNames
        {
            get { return columnNames; }
        }
    }
}
