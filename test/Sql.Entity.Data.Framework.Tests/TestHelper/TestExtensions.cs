using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Sql.Entity.Data.Core.Framework.Tests.TestHelper
{
    internal static class TestExtensions
    {
        public static void SetupRowSet(this Mock<IDataReader> reader, TestDataTable dataTable)
        {
            var columns = new List<string>();
            var rows = new List<List<Tuple<object, string, bool>>>();

            foreach (var column in dataTable.Columns)
            {
                columns.Add(column.ColumnName);
            }

            foreach (var row in dataTable.Rows)
            {
                var rowItem = new List<Tuple<object, string, bool>>();
                for (int index = 0; index < columns.Count; index++)
                {
                    rowItem.Add(new Tuple<object, string, bool>(row[index], dataTable.Columns[index].DataType.Name, dataTable.Columns[index].IsNullable));
                }
                rows.Add(rowItem);
            }
            int idx = 0;

            for (int index = 0; index < columns.Count; index++)
            {
                string columnName = columns[index];
                reader.Setup(r => r.GetOrdinal(It.Is<string>(x => x == columnName))).Returns(index);
            }

            reader.SetupGet(r => r.FieldCount).Returns(columns.Count);
            reader.Setup(r => r.Read()).Returns(() => idx < rows.Count).Callback(() => SetupNextRow(reader, rows, ref idx, columns));
        }

        private static void SetupNextRow(Mock<IDataReader> reader, List<List<Tuple<object, string, bool>>> rows, ref int index, List<string> columns)
        {
            if (index >= rows.Count)
                return;

            var row = rows[index];

            for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                var columnTuple = row[columnIndex];
                var idx = columnIndex;

                switch (columnTuple.Item2.ToLower())
                {
                    case "int32":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(int));
                        break;
                    case "int16":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(short));
                        break;
                    case "decimal":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(decimal));
                        break;
                    case "double":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(double));
                        break;
                    case "string":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(string));
                        break;
                    case "bool":
                    case "boolean":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(bool));
                        break;
                    case "datetime":
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(DateTime));
                        break;
                    default:
                        reader.Setup(r => r.GetFieldType(It.Is<int>(x => x == idx))).Returns(typeof(object));
                        break;
                }
                reader.Setup(r => r.GetValue(It.Is<int>(x => x == idx))).Returns(columnTuple.Item1);

                if (columnTuple.Item3)
                    reader.Setup(r => r.IsDBNull(It.Is<int>(x => x == idx))).Returns(columnTuple.Item1 == null);
            }
            index++;

            for (int ind = 0; ind < columns.Count; ind++)
            {
                reader.Setup(r => r.GetName(ind)).Returns(columns[ind]);
            }
        }
    }
}
