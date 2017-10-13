using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Yc.Sql.Entity.Data.Core.Framework.Cache;

namespace Yc.Sql.Entity.Data.Core.Framework.Access
{
    public class SqlDatabase : IDatabase
    {
        public const int MAX_TIMEOUT = 600;
        public const int INDEFINITE_TIMEOUT = 0;
        public const int DEFAULT_TIMEOUT = 60;

        IOptions<DatabaseConfiguration> dbOptions { get; }
        ILogger<SqlDatabase> logger { get; }

        public SqlDatabase(IOptions<DatabaseConfiguration> dbOptions, ILogger<SqlDatabase> logger)
        {
            this.dbOptions = dbOptions;
            this.logger = logger;
        }

        protected virtual IDbConnection CreateDatabaseConnection()
        {
            return new SqlConnection(dbOptions.Value.SqlConnectionString);
        }

        private IDbCommand CreateDatabaseCommand(CommandType commandType, string commandText, int timeout, params IDataParameter[] parameters)
        {
            var command = CreateDatabaseConnection().CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            command.CommandTimeout = timeout;
            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);
            return command;
        }

        public virtual IDataReader ExecuteReader(string commandText, params IDataParameter[] parameters)
        {
            return ExecuteReader(commandText, MAX_TIMEOUT, parameters);
        }

        public IDataReader ExecuteReader(string commandText, int timeout, params IDataParameter[] parameters)
        {

            logger.LogDebug($"Executing Reader with arguments commandText '{commandText}', timeout '{timeout}', parameters '{GetParametersLogText(parameters)}'");
            var command = CreateDatabaseCommand(CommandType.Text, commandText, timeout, parameters);
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public IDataReader ExecuteReaderStoredProcedure(string storedProcedureText, params IDataParameter[] parameters)
        {
            return ExecuteReaderStoredProcedure(storedProcedureText, MAX_TIMEOUT, parameters);
        }

        public IDataReader ExecuteReaderStoredProcedure(string storedProcedureText, int timeout, params IDataParameter[] parameters)
        {
            logger.LogDebug($"Executing Reader procudure with arguments storedProcedureText '{storedProcedureText}', timeout '{timeout}', parameters '{GetParametersLogText(parameters)}'");
            var command = CreateDatabaseCommand(CommandType.StoredProcedure, storedProcedureText, timeout, parameters);
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public int ExecuteNonQuery(string commandText, params IDataParameter[] parameters)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, parameters);
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteNonQuery(commandType, commandText, false, parameters);
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, bool useTransaction, params IDataParameter[] parameters)
        {
            var result = 0;

            logger.LogDebug($"Executing Non-Query with arguments commandType '{commandType}', commandText '{commandText}', useTransaction '{useTransaction}', parameters '{GetParametersLogText(parameters)}'");

            using (var connection = CreateDatabaseConnection())
            {
                var command = CreateDatabaseCommand(commandType, commandText, INDEFINITE_TIMEOUT, parameters);
                command.Connection.Open();

                if(useTransaction)
                    using (var tx = command.Connection.BeginTransaction())
                    {
                        command.Transaction = tx;
                        result = command.ExecuteNonQuery();
                    }
                else
                    result = command.ExecuteNonQuery();
            }
            
            return result;
        }

        public object ExecuteScalar(string commandText, params IDataParameter[] parameters)
        {
            return ExecuteScalar(CommandType.Text, commandText, MAX_TIMEOUT, parameters);
        }

        public object ExecuteScalar(CommandType commandType, string commandText, int timeout, params IDataParameter[] parameters)
        {
            logger.LogDebug($"Executing Scalar with arguments commandType '{commandType}', commandText '{commandText}', timeout '{timeout}', parameters '{GetParametersLogText(parameters)}'");

            using (var connection = CreateDatabaseConnection())
            {
                var command = CreateDatabaseCommand(commandType, commandText, timeout, parameters);
                command.Connection.Open();

                return command.ExecuteScalar();
            }
        }

        private string GetParametersLogText(params IDataParameter[] parameters)
        {
            var builder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                builder.Append($"Parameter(Name: {parameter.ParameterName}, Value: {parameter.Value}),");
            }
            return builder.ToString().Trim(',');
        }
    }
}
