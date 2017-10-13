//using System;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;

//namespace Yc.Sql.Entity.Data.Core.Framework.Helper
//{
//    public class BinarySerializer : IBinarySerializer, IDisposable
//    {
//        BinaryFormatter binaryFormatter = new BinaryFormatter();
//        public object ConvertByteArrayToObject(byte[] byteArrayData)
//        {
//            using (var memoryStream = new MemoryStream())
//            {
//                memoryStream.Write(byteArrayData, 0, byteArrayData.Length);
//                memoryStream.Seek(0, SeekOrigin.Begin);
//                return binaryFormatter.Deserialize(memoryStream);
//            }
//        }

//        public byte[] ConvertObjectToByteArray(object objectData)
//        {
//            using (var memoryStream = new MemoryStream())
//            {
//                binaryFormatter.Serialize(memoryStream, objectData);
//                return memoryStream.ToArray();
//            }
//        }

//        private bool disposedValue = false; 

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    binaryFormatter = null;
//                }
//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }
//    }

//    public interface IBinarySerializer
//    {
//        object ConvertByteArrayToObject(byte[] byteArrayData);
//        byte[] ConvertObjectToByteArray(object objectData);
//    }
//}
