using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Yc.Sql.Entity.Data.Core.Framework.Helper;

namespace Sql.Entity.Data.Core.Framework.Tests.Helper
{
    public class ConcurrentProcessorTests
    {
        private Mock<ILogger<ConcurrentProcessor>> logger;
        private IConcurrentProcessor processor;

        public ConcurrentProcessorTests()
        {
            logger = new Mock<ILogger<ConcurrentProcessor>>();
            processor = new ConcurrentProcessor(logger.Object);
        }

        [Fact]
        public void Test_ExecuteInThreads_Default_Concurrent()
        {
            var threadClassifierCollection = new Dictionary<dynamic, dynamic[]>
            {
                { "1", new dynamic[] { 4, 3, 4, 3 } },
                { "2", new dynamic[] { 5, 5, 5, 5 } }
            };

            Stopwatch sw = new Stopwatch();

            sw.Start();
            var output = processor.ExecuteInThreads(TestMethodToAdd, threadClassifierCollection);
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 10000);

            Assert.NotNull(output);
            Assert.Equal(output.Count, 2);
            Assert.Equal(output["1"], 10);
            Assert.Equal(output["2"], 15);
        }

        [Fact]
        public void Test_ExecuteInThreads_ConcurrentAsFalse()
        {
            var threadClassifierCollection = new Dictionary<dynamic, dynamic[]>
            {
                { "1", new dynamic[] { 4, 3, 4, 3 } },
                { "2", new dynamic[] { 5, 5, 5, 5 } }
            };

            Stopwatch sw = new Stopwatch();

            sw.Start();
            var output = processor.ExecuteInThreads(TestMethodToAdd, threadClassifierCollection, false);
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds > 9000);

            Assert.NotNull(output);
            Assert.Equal(output.Count, 2);
            Assert.Equal(output["1"], 10);
            Assert.Equal(output["2"], 15);
        }

        private int TestMethodToAdd(params dynamic[] methodArguments)
        {
            Thread.Sleep(methodArguments[0] * 1000);

            int sum = 0;

            for (int i = 1; i < methodArguments.Length; i++)
                sum += methodArguments[i];

            return sum;
        }
    }
}
