using Microsoft.Extensions.Logging;
using Yc.Sql.Entity.Data.Core.Framework.Controller;
using Yc.Sql.Entity.Data.Core.Framework.Mapper;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using Xunit;
using Moq;
using Yc.Sql.Entity.Data.Core.Framework.Model.Controller;
using System;
using System.Collections.Generic;
using Yc.Sql.Entity.Data.Core.Framework.Helper;

namespace Sql.Entity.Data.Core.Framework.Tests.Controller
{
    public class BaseDataControllerTests
    {
        Mock<ILogger<BaseDataController>> logger;
        Mock<IDataMapper> dataMapper;
        IDataController dataController;

        public BaseDataControllerTests()
        {
            logger = new Mock<ILogger<BaseDataController>>();
            dataMapper = new Mock<IDataMapper>();
        }

        [Fact]
        public void Test_SubmitChanges_SingleDataSet()
        {
            IBaseContext entity = new TestContext() { Id = 1, Name = "Foo" };
            entity.ControllerFunction = TestFunction.InsertTestData;
            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(entity)).Returns(1);

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entity, requestInfo);

            dataMapper.Verify(dm => dm.SubmitData(entity), Times.Once);

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal((int)responseInfo.Data, 1);
            Assert.Equal(responseInfo.Status, Status.Success);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 0);
        }

        [Fact]
        public void Test_SubmitChanges_SingleDataSet_OnError()
        {
            IBaseContext entity = new TestContext() { Id = 1, Name = "Foo" };
            entity.ControllerFunction = TestFunction.InsertTestData;
            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(It.IsAny<IBaseContext>())).Callback(() => { throw new Exception(); });

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entity, requestInfo);

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal(responseInfo.Status, Status.Failure);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 1);
        }

        [Fact]
        public void Test_SubmitChanges_GenericSingleDataSet()
        {
            TestContext entity = new TestContext() { Id = 1, Name = "Foo" };
            entity.ControllerFunction = TestFunction.InsertTestData;
            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(entity)).Returns(1);

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entity, requestInfo);

            dataMapper.Verify(dm => dm.SubmitData(entity), Times.Once);

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal((int)responseInfo.Data, 1);
            Assert.Equal(responseInfo.Status, Status.Success);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 0);
        }

        [Fact]
        public void Test_SubmitChanges_GenericSingleDataSet_OnError()
        {
            TestContext entity = new TestContext() { Id = 1, Name = "Foo" };
            entity.ControllerFunction = TestFunction.InsertTestData;
            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(It.IsAny<IBaseContext>())).Callback(() => { throw new Exception(); });

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entity, requestInfo);

            dataMapper.Verify(dm => dm.SubmitData(entity), Times.Once);

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal(responseInfo.Status, Status.Failure);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 1);
        }

        [Fact]
        public void Test_SubmitChanges_CollectionDataSet()
        {
            List<TestContext> entities = new List<TestContext> { new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.InsertTestDatas }, new TestContext() { Id = 2, Name = "Fooo", ControllerFunction = TestFunction.InsertTestDatas } };

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(entities));

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entities, requestInfo);

            dataMapper.Verify(dm => dm.SubmitData(entities), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal((bool)responseInfo.Data, true);
            Assert.Equal(responseInfo.Status, Status.Success);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 0);
        }

        [Fact]
        public void Test_SubmitChanges_CollectionDataSet_OnError()
        {
            List<TestContext> entities = new List<TestContext> { new TestContext() { Id = 1, Name = "Foo", ControllerFunction = TestFunction.InsertTestDatas }, new TestContext() { Id = 2, Name = "Fooo", ControllerFunction = TestFunction.InsertTestDatas } };

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };

            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            dataMapper.Setup(dm => dm.SubmitData(entities)).Callback(() => { throw new Exception(); });

            IDataResponseInfo responseInfo = dataController.SubmitChanges(entities, requestInfo);

            dataMapper.Verify(dm => dm.SubmitData(entities), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Equal(responseInfo.Status, Status.Failure);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 1);
        }

        [Fact]
        public void Test_GetEntity()
        {
            TestContext entity = new TestContext() { Id = 1 };
            entity.ControllerFunction = TestFunction.GetById;

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };
            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            TestContext expectedEntity = new TestContext() { Id = 1 , Name = "Foo" };

            dataMapper.Setup(dm => dm.GetDataItem<TestContext>(entity)).Returns(expectedEntity);

            IDataResponseInfo responseInfo = dataController.GetEntity<TestContext>(entity, requestInfo);

            dataMapper.Verify(dm => dm.GetDataItem<TestContext>(entity), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.NotNull(responseInfo.Data);
            Assert.IsType<TestContext>(responseInfo.Data);
            Assert.Equal(((TestContext)responseInfo.Data).Id, 1);
            Assert.Equal(((TestContext)responseInfo.Data).Name, "Foo");
            Assert.Equal(responseInfo.Status, Status.Success);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 0);
        }

        [Fact]
        public void Test_GetEntity_OnError()
        {
            TestContext entity = new TestContext() { Id = 1 };
            entity.ControllerFunction = TestFunction.GetById;

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };
            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            TestContext expectedEntity = new TestContext() { Id = 1, Name = "Foo" };

            dataMapper.Setup(dm => dm.GetDataItem<TestContext>(entity)).Returns(expectedEntity).Callback(() => { throw new Exception(); }); 

            IDataResponseInfo responseInfo = dataController.GetEntity<TestContext>(entity, requestInfo);

            dataMapper.Verify(dm => dm.GetDataItem<TestContext>(entity), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Null(responseInfo.Data);
            Assert.Equal(responseInfo.Status, Status.Failure);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 1);
        }

        [Fact]
        public void Test_GetEntities()
        {
            TestContext entity = new TestContext() { ControllerFunction = TestFunction.GetAll };

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };
            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            List<TestContext> expectedEntity = new List<TestContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            dataMapper.Setup(dm => dm.GetDataItems<TestContext>(entity)).Returns(expectedEntity);

            IDataResponseInfo responseInfo = dataController.GetEntities<TestContext>(entity, requestInfo);

            dataMapper.Verify(dm => dm.GetDataItems<TestContext>(entity), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.NotNull(responseInfo.Data);
            Assert.IsType<List<TestContext>>(responseInfo.Data);
            Assert.Equal(((List<TestContext>)responseInfo.Data).Count, 2);
            Assert.Equal(((List<TestContext>)responseInfo.Data)[0].Id, 1);
            Assert.Equal(((List<TestContext>)responseInfo.Data)[0].Name, "Foo");
            Assert.Equal(((List<TestContext>)responseInfo.Data)[1].Id, 2);
            Assert.Equal(((List<TestContext>)responseInfo.Data)[1].Name, "Fooo");
            Assert.Equal(responseInfo.Status, Status.Success);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 0);
        }

        [Fact]
        public void Test_GetEntities_OnError()
        {
            TestContext entity = new TestContext() { ControllerFunction = TestFunction.GetAll };

            IDataRequestInfo requestInfo = new DataRequestInfo { CorrelationId = Guid.NewGuid().ToString(), RequestorName = "Faa" };
            dataController = new BaseDataControllerStub(dataMapper.Object, logger.Object);

            List<TestContext> expectedEntity = new List<TestContext> { new TestContext() { Id = 1, Name = "Foo" }, new TestContext() { Id = 2, Name = "Fooo" } };

            dataMapper.Setup(dm => dm.GetDataItems<TestContext>(entity)).Returns(expectedEntity).Callback(() => { throw new Exception(); });

            IDataResponseInfo responseInfo = dataController.GetEntities<TestContext>(entity, requestInfo);

            dataMapper.Verify(dm => dm.GetDataItems<TestContext>(entity), Times.Exactly(1));

            Assert.Equal(responseInfo.CorrelationId, requestInfo.CorrelationId);
            Assert.Equal(responseInfo.HostName, Environment.MachineName);
            Assert.Null(responseInfo.Data);
            Assert.Equal(responseInfo.Status, Status.Failure);

            Assert.Equal(((BaseDataControllerStub)dataController).InitializeResponseCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCompletionCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).VerifyResponseStatusCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnStartCount, 1);
            Assert.Equal(((BaseDataControllerStub)dataController).OnCustomMessageCount, 0);
            Assert.Equal(((BaseDataControllerStub)dataController).OnExceptionCount, 1);
        }
    }

    internal class BaseDataControllerStub : BaseDataController
    {
        public int VerifyResponseStatusCount { get; set; }
        public int OnCompletionCount { get; set; }
        public int InitializeResponseCount { get; set; }
        public int OnCustomMessageCount { get; set; }
        public int OnStartCount { get; set; }
        public int OnExceptionCount { get; set; }

        public BaseDataControllerStub(IDataMapper dataMapper, ILogger<BaseDataController> logger) 
            : base(dataMapper, logger)
        {
        }

        public BaseDataControllerStub(IDataMapper dataMapper, IConcurrentProcessor concurrentProcessor, ILogger<BaseDataController> logger)
            : base(dataMapper, concurrentProcessor, logger)
        {
        }

        protected override IDataResponseInfo InitializeResponse(IDataResponseInfo response, IDataRequestInfo request)
        {
            InitializeResponseCount++;
            return base.InitializeResponse(response, request);
        }

        protected override bool VerifyResponseStatus(IDataResponseInfo response, bool throwException)
        {
            VerifyResponseStatusCount++;
            return base.VerifyResponseStatus(response, throwException);
        }

        protected override void OnCompletion(IDataController controller, IDataMapper mapper, IBaseContext context, IDataRequestInfo request)
        {
            OnCompletionCount++;
            base.OnCompletion(controller, mapper, context, request);
        }

        protected override void OnCustomMessage(IDataController controller, IDataMapper mapper, IBaseContext context, IDataRequestInfo request, string message)
        {
            OnCustomMessageCount++;
            base.OnCustomMessage(controller, mapper, context, request, message);
        }

        protected override void OnStart(IDataController controller, IDataMapper mapper, IBaseContext context, IDataRequestInfo request, IDataResponseInfo response)
        {
            OnStartCount++;
            base.OnStart(controller, mapper, context, request, response);
        }

        protected override void OnException(IDataController controller, IDataMapper mapper, IBaseContext context, IDataRequestInfo request, IDataResponseInfo response, Exception exception)
        {
            OnExceptionCount++;
            base.OnException(controller, mapper, context, request, response, exception);
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
