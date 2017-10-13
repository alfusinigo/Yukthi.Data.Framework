
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sql.Entity.Data.Core.Framework.Tests.TestHelper
{
    internal class TestDataRow
    {
        private readonly List<object> values;

        public object this[int index]
        {
            get
            {
                return values[index];
            }
        }

        public bool IsNullable { get; set; }

        public TestDataRow(params object[] rowValues)
        {
            values = rowValues.ToList();
        }
    }
}
