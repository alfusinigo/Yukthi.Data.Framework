using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Yc.Sql.Entity.Data.Core.Framework.Helper;
using Yc.Sql.Entity.Data.Core.Framework.Mapper;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;

namespace Sql.Entity.Data.Core.Framework.Tests.Mapper
{
    //TODO: Add tests for all scenarios
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

            List<IBaseContext> expectedEntity = new List<IBaseContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItems(context, typeof(TestContext));

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(result.ToList().Count, 2);

            Assert.Equal(((TestContext)result.ToList()[0]).Name, "Foo");
            Assert.Equal(((TestContext)result.ToList()[0]).Id, 1);
            Assert.Equal(((TestContext)result.ToList()[1]).Name, "Fooo");
            Assert.Equal(((TestContext)result.ToList()[1]).Id, 2);
        }

        //TODO: Mock database & reader
        //[Fact]
        public void Test_GetDataItems_WithCacheEnabled_NotContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            List<IBaseContext> expectedEntity = new List<IBaseContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItems(context, typeof(TestContext));

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(result.ToList().Count, 2);

            Assert.Equal(((TestContext)result.ToList()[0]).Name, "Foo");
            Assert.Equal(((TestContext)result.ToList()[0]).Id, 1);
            Assert.Equal(((TestContext)result.ToList()[1]).Name, "Fooo");
            Assert.Equal(((TestContext)result.ToList()[1]).Id, 2);
        }

        //TODO: Mock database & reader
        //[Fact]
        public void Test_GetDataItems_WithoutCacheEnabled()
        {
            var mapper = new DataMapperBaseStub(database.Object, logger.Object);

            List<IBaseContext> expectedEntity = new List<IBaseContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItems(context, typeof(TestContext));

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
        public void Test_GetDataItem_WithCacheEnabled_ContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetById };

            cacheRepository.Setup(c => c.ContainsValue("select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItem(context, typeof(TestContext));

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(((TestContext)result).Name, "Foo");
            Assert.Equal(((TestContext)result).Id, 1);
        }

        //TODO: Mock database & reader
        //[Fact]
        public void Test_GetDataItem_WithCacheEnabled_NotContainingCachedData()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItem(context, typeof(TestContext));

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(((TestContext)result).Name, "Foo");
            Assert.Equal(((TestContext)result).Id, 1);
        }

        //TODO: Mock database & reader
        //[Fact]
        public void Test_GetDataItem_WithoutCacheEnabled()
        {
            var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            var expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            var context = new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll };

            cacheRepository.Setup(c => c.ContainsValue("select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>())).Returns(true);

            cacheRepository.SetupGet(c => c["select * from test where id=@id", "test", It.IsAny<IEnumerable<IDataParameter>>()]).Returns(new BinarySerializer().ConvertObjectToByteArray(expectedEntity));

            var result = mapper.GetDataItem(context, typeof(TestContext));

            Assert.Equal(mapper.GetAllCaseCount, 1);

            cacheRepository.Verify();

            Assert.NotNull(result);

            Assert.Equal(((TestContext)result).Name, "Foo");
            Assert.Equal(((TestContext)result).Id, 1);
        }

        [Fact]
        public void Test_SubmitData_OneDataSet()
        {
        }

        [Fact]
        public void Test_SubmitData_MultipleDataSets()
        {
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
        public int InsertTestDataCount { get; set; }

        public override void SetFunctionSpecificEntityMappings(IBaseContext context)
        {
            switch (context.ControllerFunction)
            {
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
                    GetAllCaseCount++;
                    break;
                default:
                    break;
            }
        }
    }

    [Serializable]
    internal class TestContext : BaseContext, ITestContext
    {
        public int Id { get; set; }
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
