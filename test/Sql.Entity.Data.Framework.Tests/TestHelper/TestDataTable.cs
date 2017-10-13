using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sql.Entity.Data.Core.Framework.Tests.TestHelper
{
    internal class TestDataTable
    {
        public List<TestDataColumn> Columns { get; private set; } = new List<TestDataColumn>();

        public List<TestDataRow> Rows { get; private set; } = new List<TestDataRow>();

        public TestDataTable()
        {
        }

        public TestDataTable(List<TestDataColumn> columns, List<TestDataRow> rows)
        {
            Columns = columns;
            Rows = rows;
        }
    }
}
