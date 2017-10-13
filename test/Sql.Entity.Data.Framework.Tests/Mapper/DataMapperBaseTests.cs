using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Sql.Entity.Data.Core.Framework.Tests.TestHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using Xunit;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Yc.Sql.Entity.Data.Core.Framework.Mapper;
using Yc.Sql.Entity.Data.Core.Framework.Model.Attributes;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;

namespace Sql.Entity.Data.Core.Framework.Tests.Mapper
{
    public class DataMapperBaseTests
    {
        private Mock<IDatabase> database;
        private Mock<ICacheRepository> cacheRepository;
        private Mock<ILogger<DataMapperBase>> logger;

        public DataMapperBaseTests()
        {
            database = new Mock<IDatabase>();
            cacheRepository = new Mock<ICacheRepository>();
            logger = new Mock<ILogger<DataMapperBase>>();
        }

        [Fact]
        public void Test_GetDataItems_WithCacheEnabled_ContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            List<TestContext> expectedEntity = new List<TestContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue($"select * from test-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c[$"select * from test-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(JsonConvert.SerializeObject(expectedEntity));

            var result = mapper.GetDataItems<TestContext>(context);

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(result.ToList().Count, 2);

            Assert.Equal(((TestContext)result.ToList()[0]).Name, "Foo");
            Assert.Equal(((TestContext)result.ToList()[0]).Id, 1);
            Assert.Equal(((TestContext)result.ToList()[1]).Name, "Fooo");
            Assert.Equal(((TestContext)result.ToList()[1]).Id, 2);
        }

        [Fact]
        public void Test_GetDataItems_WithoutCacheEnabled()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new TestContext() { ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(false);

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(object)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));
            dataTable.Rows.Add(new TestDataRow(2, "Fooo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItems<TestContext>(context);

            Assert.Equal(mapper.GetAllCaseCount, 2);

            cacheRepository.Verify();
            database.Verify();

            Assert.NotNull(result);

            Assert.Equal(result.Count, 2);

            Assert.Equal(result[0].Name, "Foo");
            Assert.Equal(result[0].Id, 1);
            Assert.Equal(result[1].Name, "Fooo");
            Assert.Equal(result[1].Id, 2);
        }

        [Fact]
        public void Test_GetDataItems_WithoutCacheEnabled_DynamicDataType()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new TestContext() { ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(false);

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(object)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));
            dataTable.Rows.Add(new TestDataRow(2, "Fooo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItems<dynamic>(context);

            var json = JsonConvert.SerializeObject(result);

            Assert.Equal(json, "[{\"ID\":1,\"NAME\":\"Foo\"},{\"ID\":2,\"NAME\":\"Fooo\"}]");
            Assert.Equal(mapper.GetAllCaseCount, 2);

            cacheRepository.Verify();
            database.Verify();

            Assert.NotNull(result);

            Assert.Equal(result.Count, 2);
        }

        [Fact]
        public void Test_GetDataItem_WithCacheEnabled_ContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetById };

            cacheRepository.Setup(c => c.ContainsValue($"select * from test where id=@id-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c[$"select * from test where id=@id-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(JsonConvert.SerializeObject(expectedEntity));

            var result = mapper.GetDataItem<TestContext>(context);

            Assert.Equal(mapper.GetByIdCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(((TestContext)result).Name, "Foo");
            Assert.Equal(((TestContext)result).Id, 1);
        }

        [Fact]
        public void Test_GetDataItem_WithCacheEnabled_NotContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetById };

            cacheRepository.Setup(c => c.ContainsValue($"select * from test where id=@id-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(false);

            cacheRepository.SetupGet(c => c[$"select * from test where id=@id-{typeof(TestContext).FullName}", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(JsonConvert.SerializeObject(expectedEntity));

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(string)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test where id=@id", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItem<TestContext>(context);

            Assert.Equal(mapper.GetByIdCaseCount, 2);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(((TestContext)result).Name, "Foo");
            Assert.Equal(((TestContext)result).Id, 1);
        }

        [Fact]
        public void Test_GetDataItem_WithCacheEnabled_NotContainingCachedData_Dynamic()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetById };

            cacheRepository.Setup(c => c.ContainsValue($"select * from test where id=@id-System.Object", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(false);

            cacheRepository.SetupGet(c => c[$"select * from test where id=@id-System.Object", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns("{\"ID\":1,\"NAME\":\"Foo\"}");

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(string)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test where id=@id", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItem<dynamic>(context);

            Assert.Equal(mapper.GetByIdCaseCount, 2);

            cacheRepository.Verify();

            Assert.NotNull(result);

            var json = JsonConvert.SerializeObject(result);

            Assert.Equal(json, "{\"ID\":1,\"NAME\":\"Foo\"}");
        }

        [Fact]
        public void Test_GetDataItem_WithoutCacheEnabledOrCacheRepoNull()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new TestContext() { ControllerFunction = TestFunction.GetById };

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(string)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test where id=@id", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItem<TestContext>(context);

            cacheRepository.Verify();
            database.Verify();

            Assert.Equal(mapper.GetByIdCaseCount, 2);

            Assert.NotNull(result);

            Assert.Equal(result.Name, "Foo");
            Assert.Equal(result.Id, 1);
        }

        [Fact]
        public void Test_GetDataItem_WithoutCacheEnabledOrCacheRepoNull_Dynamic()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new TestContext() { ControllerFunction = TestFunction.GetById };

            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("NAME", typeof(string)));

            dataTable.Rows.Add(new TestDataRow(1, "Foo"));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            database.Setup(db => db.ExecuteReader("select * from test where id=@id", It.IsAny<int>(), It.IsAny<IDataParameter[]>())).Returns(reader.Object);

            var result = mapper.GetDataItem<dynamic>(context);

            Assert.Equal(mapper.GetByIdCaseCount, 2);

            cacheRepository.Verify();
            database.Verify();

            Assert.NotNull(result);

            var json = JsonConvert.SerializeObject(result);

            Assert.Equal(json, "{\"ID\":1,\"NAME\":\"Foo\"}");
        }

        [Fact]
        public void Test_SubmitData_OneDataSet()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.InsertTestData };

            database.Setup(db => db.ExecuteScalar(It.Is<CommandType>(x => x == CommandType.StoredProcedure), "dbo.Insert", context.Timeout, It.IsAny<IDataParameter[]>())).Returns(100);

            var result = mapper.SubmitData(context);

            database.Verify();

            Assert.Equal(result, 100);

            Assert.Equal(mapper.InsertTestDataCount, 1);
        }

        [Fact]
        public void Test_SubmitData_MultipleDataSets()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            var context = new List<TestContext>() { new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.InsertTestData }, new TestContext() { Id = 2, Name = "Fooo", ControllerFunction = TestFunction.InsertTestData }, new TestContext() { Id = 3, Name = "Foooo", ControllerFunction = TestFunction.InsertTestData } };

            database.Setup(db => db.ExecuteScalar(It.Is<CommandType>(x => x == CommandType.StoredProcedure), "dbo.Insert", SqlDatabase.MAX_TIMEOUT, It.IsAny<IDataParameter[]>()));

            mapper.SubmitData(context);

            database.Verify();

            Assert.Equal(mapper.InsertTestDataCount, 3);
        }

        private static Mock<IDataReader> GetMockDataReader()
        {
            var dataTable = new TestDataTable();
            dataTable.Columns.Add(new TestDataColumn("ID", typeof(int)));
            dataTable.Columns.Add(new TestDataColumn("TEST_NAME", typeof(string)));
            dataTable.Columns.Add(new TestDataColumn("TEST_DOB", typeof(DateTime)));
            dataTable.Columns.Add(new TestDataColumn("TEST_IS_TRUE", typeof(bool)));

            dataTable.Rows.Add(new TestDataRow(1, "TestingName", DateTime.Now.Date, true));
            dataTable.Rows.Add(new TestDataRow(2, "TestingName2", DateTime.Now.Date.AddDays(1), false));

            var reader = new Mock<IDataReader>();
            reader.SetupRowSet(dataTable);

            return reader;
        }
    }

    internal class DataMapperBaseStub : DataMapperBase
    {
        public DataMapperBaseStub(IDatabase database, ICacheRepository cacheRepository, ILogger<DataMapperBase> logger)
            : base(database, cacheRepository, logger)
        {
        }

        public DataMapperBaseStub(IDatabase database, ILogger<DataMapperBase> logger)
            : base(database, logger)
        {
        }

        public int GetAllCaseCount { get; set; }
        public int GetByIdCaseCount { get; set; }
        public int InsertTestDataCount { get; set; }

        public override void SetFunctionSpecificEntityMappings(IBaseContext context)
        {
            switch (context.ControllerFunction)
            {
                case TestFunction.InsertTestDatas:
                case TestFunction.InsertTestData:
                    context.Command = "dbo.Insert";
                    context.CommandType = CommandType.StoredProcedure;
                    InsertTestDataCount++;
                    break;
                case TestFunction.GetAll:
                    context.Command = "select * from test";
                    context.CommandType = CommandType.Text;
                    context.DependingDbTableNamesInCsv = "test";
                    GetAllCaseCount++;
                    break;
                case TestFunction.GetById:
                    context.Command = "select * from test where id=@id";
                    context.CommandType = CommandType.Text;
                    context.DependingDbTableNamesInCsv = "test";
                    GetByIdCaseCount++;
                    break;
                default:
                    break;
            }
        }
    }

    internal class TestContext : BaseContext, ITestContext
    {
        [ColumnNames("ID")]
        public int Id { get; set; }

        [ColumnNames("NAME")]
        public string Name { get; set; }
    }

    internal interface ITestContext
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    internal class TestFunction : BaseFunction
    {
        public const string InsertTestData = "InsertTestData";
        public const string InsertTestDatas = "InsertTestDatas";
    }
}
