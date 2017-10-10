using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
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

        //[Fact]
        public void Test_GetDataItems()
        {
            //var mapper = new DataMapperBaseStub(database.Object, cacheRepository.Object, logger.Object);

            //List<IBaseContext> expectedEntity = new List<IBaseContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            //database.Setup(db=>db.ExecuteReader)
            //mapper.GetDataItems(new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.GetAll }, typeof(TestContext));
        }
    }

    internal class DataMapperBaseStub : DataMapperBase
    {
        public DataMapperBaseStub(IDatabase database, ICacheRepository cacheRepository, ILogger<DataMapperBase> logger) 
            : base(database, cacheRepository, logger)
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
                    GetAllCaseCount++;
                    break;
                default:
                    break;
            }
        }
    }

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
