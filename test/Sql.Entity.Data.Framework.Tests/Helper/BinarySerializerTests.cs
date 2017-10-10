using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Yc.Sql.Entity.Data.Core.Framework.Helper;

namespace Sql.Entity.Data.Core.Framework.Tests.Helper
{
    public class BinarySerializerTests
    {
        [Fact]
        public void Test_Serialize_And_Deserialize()
        {
            Sample sample = new Sample { Id = 1, Name = "Foo" };

            var serializedByteArrayData = new BinarySerializer().ConvertObjectToByteArray(sample);

            var deserializedSample = (Sample)new BinarySerializer().ConvertByteArrayToObject(serializedByteArrayData);

            Assert.NotNull(deserializedSample);
            Assert.IsType<Sample>(deserializedSample);
            Assert.Equal(deserializedSample.Id, sample.Id);
            Assert.Equal(deserializedSample.Name, sample.Name);
        }
    }

    [Serializable]
    internal class Sample
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
