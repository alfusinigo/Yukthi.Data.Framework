using System.Data;

namespace Yc.Sql.Entity.Data.Core.Framework.Access
{
    public interface IDatabase
    {
        IDataReader ExecuteReader(string command, params IDataParameter[] parameters);
        IDataReader ExecuteReader(string command, int timeout, params IDataParameter[] parameters);

        IDataReader ExecuteReaderStoredProcedure(string storedProcedure, params IDataParameter[] parameters);
        IDataReader ExecuteReaderStoredProcedure(string storedProcedure, int timeout, params IDataParameter[] parameters);

        int ExecuteNonQuery(string command, params IDataParameter[] parameters);
        int ExecuteNonQuery(CommandType commandType, string command, params IDataParameter[] parameters);
        int ExecuteNonQuery(CommandType commandType, string command, bool useTransaction, params IDataParameter[] parameters);

        object ExecuteScalar(string command, params IDataParameter[] parameters);
        object ExecuteScalar(CommandType commandType, string command, int timeout, params IDataParameter[] parameters);
    }
}
