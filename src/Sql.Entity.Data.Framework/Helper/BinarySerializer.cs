using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Yc.Sql.Entity.Data.Core.Framework.Helper
{
    public class BinarySerializer : IBinarySerializer
    {
        public object ConvertByteArrayToObject(byte[] byteArrayData)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArrayData, 0, byteArrayData.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        public byte[] ConvertObjectToByteArray(object objectData)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, objectData);
                return memoryStream.ToArray();
            }
        }
    }

    public interface IBinarySerializer
    {
        object ConvertByteArrayToObject(byte[] byteArrayData);
        byte[] ConvertObjectToByteArray(object objectData);
    }
}
