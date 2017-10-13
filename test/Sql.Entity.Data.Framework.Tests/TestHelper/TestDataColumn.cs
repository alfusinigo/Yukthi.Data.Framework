using System;
using System.Collections.Generic;
using System.Text;

namespace Sql.Entity.Data.Core.Framework.Tests.TestHelper
{
    internal class TestDataColumn
    {
        public string ColumnName { get; set; }

        public Type DataType { get; set; }

        public bool IsNullable { get; set; }

        public TestDataColumn(string columnName, Type dataType, bool isNullable = false)
        {
            ColumnName = columnName;
            DataType = dataType;
            IsNullable = isNullable;
        }
    }
}
