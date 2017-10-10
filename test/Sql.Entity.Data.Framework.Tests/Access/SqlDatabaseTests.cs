using Xunit;
using Moq;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using System.Data;
using System.Data.SqlClient;

namespace Sql.Entity.Data.Core.Framework.Tests.Access
{
    public class SqlDatabaseTests
    {
        Mock<IOptions<DatabaseConfiguration>> dbOptions;
        Mock<ILogger<SqlDatabase>> logger;
        IDatabase database;

        public SqlDatabaseTests()
        {
            dbOptions = new Mock<IOptions<DatabaseConfiguration>>();
            logger = new Mock<ILogger<SqlDatabase>>();
        }

        [Fact]
        public void Test_ExecuteReader()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection))).Returns(reader.Object);

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteReader("text", new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection)), Times.Once);
            connection.Verify(con => con.CreateCommand(), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.Text));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "text"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.MAX_TIMEOUT));
        }

        [Fact]
        public void Test_ExecuteReader_WithTimeout()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection))).Returns(reader.Object);

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteReader("text", SqlDatabase.INDEFINITE_TIMEOUT, new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection)), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.Text));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "text"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.INDEFINITE_TIMEOUT));
            connection.Verify(con => con.CreateCommand(), Times.Once);
        }

        [Fact]
        public void Test_ExecuteReaderStoredProcedure()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection))).Returns(reader.Object);

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteReaderStoredProcedure("sp", new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection)), Times.Once);
            connection.Verify(con => con.CreateCommand(), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.StoredProcedure));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "sp"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.MAX_TIMEOUT));
        }

        [Fact]
        public void Test_ExecuteReaderStoredProcedure_WithTimeout()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection))).Returns(reader.Object);

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteReaderStoredProcedure("sp", SqlDatabase.INDEFINITE_TIMEOUT, new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteReader(It.Is<CommandBehavior>(cb => cb == CommandBehavior.CloseConnection)), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.StoredProcedure));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "sp"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.INDEFINITE_TIMEOUT));
            connection.Verify(con => con.CreateCommand(), Times.Once);
        }

        [Fact]
        public void Test_ExecuteNonQueryWithTransaction()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            connection.Setup(con => con.BeginTransaction()).Returns(transaction.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteNonQuery());

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteNonQuery(CommandType.Text, "query", true, new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteNonQuery(), Times.Once);
            connection.Verify(con => con.CreateCommand(), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.Text));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "query"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.INDEFINITE_TIMEOUT));
            command.VerifySet(com => com.Transaction = It.Is<IDbTransaction>(value => value == transaction.Object));
        }

        [Fact]
        public void Test_ExecuteNonQueryWithoutTransaction()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteNonQuery());

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteNonQuery(CommandType.Text, "query", new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteNonQuery(), Times.Once);
            connection.Verify(con => con.CreateCommand(), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.Text));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "query"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.INDEFINITE_TIMEOUT));
            command.VerifySet(com => com.Transaction = It.IsAny<IDbTransaction>(), Times.Never);
        }

        [Fact]
        public void Test_ExecuteScalar()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(con => con.CreateCommand()).Returns(command.Object);
            connection.Setup(con => con.BeginTransaction()).Returns(transaction.Object);
            command.Setup(com => com.Parameters).Returns(new SqlCommand().Parameters);
            command.Setup(com => com.Connection).Returns(connection.Object);
            command.Setup(com => com.ExecuteNonQuery());

            database = new SqlDatabaseStub(dbOptions.Object, logger.Object, connection.Object);

            database.ExecuteScalar("select", new SqlParameter[] { new SqlParameter("param1", "param1Value") });

            command.Verify(com => com.ExecuteScalar(), Times.Once);
            connection.Verify(con => con.CreateCommand(), Times.Once);
            command.VerifySet(com => com.CommandType = It.Is<CommandType>(value => value == CommandType.Text));
            command.VerifySet(com => com.CommandText = It.Is<string>(value => value == "select"));
            command.VerifySet(com => com.CommandTimeout = It.Is<int>(value => value == SqlDatabase.MAX_TIMEOUT));
        }
    }

    internal class SqlDatabaseStub : SqlDatabase
    {
        IDbConnection connection;

        public SqlDatabaseStub(IOptions<DatabaseConfiguration> dbOptions, ILogger<SqlDatabase> logger, IDbConnection connection)
            : base(dbOptions, logger)
        {
            this.connection = connection;
        }

        protected override IDbConnection CreateDatabaseConnection()
        {
            return connection;
        }
    }
}
