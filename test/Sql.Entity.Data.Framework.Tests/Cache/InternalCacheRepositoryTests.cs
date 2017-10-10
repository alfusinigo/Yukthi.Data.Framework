using Xunit;
using Moq;
using Yc.Sql.Entity.Data.Core.Framework.Cache;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SqlClient;
using System.Data;
using System.Threading;

namespace Sql.Entity.Data.Core.Framework.Tests.Cache
{
    public class InternalCacheRepositoryTests
    {
        IOptions<InternalCacheConfiguration> options;
        Mock<ILogger<InternalCacheRepository>> logger;
        IMemoryCache memoryCache;
        Mock<IDatabase> database;
        ICacheRepository repository;
        string getModifiedTimestampQuery = @"WITH TABLENAMES(NAME) AS (
                                                    SELECT NAME = VALUE FROM dbo.CSVTOTABLE('{0}')) 
                                                    SELECT MAX(ISNULL(LAST_USER_UPDATE,
                                                    CONVERT(DATETIME,'1/1/1901'))) FROM sys.dm_db_index_usage_stats S 
                                                    RIGHT OUTER JOIN TABLENAMES T ON S.OBJECT_ID=OBJECT_ID(T.NAME) 
                                                    WHERE DATABASE_ID = DB_ID();";

        public InternalCacheRepositoryTests()
        {

            logger = new Mock<ILogger<InternalCacheRepository>>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            database = new Mock<IDatabase>();
        }

        [Fact]
        public void Test_SetAndGetAndContainsCacheWithoutDbDependency()
        {
            options = Options.Create(new InternalCacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableInMemorySlidingExpiration = true,
                ExpirationInSeconds = 60
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new InternalCacheRepository(database.Object, memoryCache, options, logger.Object);

            Assert.False(repository.ContainsValue(commandText, parameters));

            repository[commandText, parameters] = "Sample Data Cached";

            Assert.True(repository.ContainsValue(commandText, parameters));

            database.Verify(db => db.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IDataParameter[]>()), Times.Never);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
        }

        [Fact]
        public void Test_SetAndGetAndContainsCacheWithDbDependency()
        {
            options = Options.Create(new InternalCacheConfiguration
            {
                EnableDatabaseChangeRefresh = true,
                EnableInMemorySlidingExpiration = true,
                ExpirationInSeconds = 60
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new InternalCacheRepository(database.Object, memoryCache, options, logger.Object);

            Assert.False(repository.ContainsValue(commandText, "table1,table2", parameters));

            repository[commandText, "table1,table2", parameters] = "Sample Data Cached";

            Assert.True(repository.ContainsValue(commandText, "table1,table2", parameters));

            database.Verify(db => db.ExecuteScalar(It.Is<CommandType>(val => val == CommandType.Text), It.Is<string>(val => val == string.Format(getModifiedTimestampQuery, "table1,table2")), It.Is<int>(val => val == 0), It.IsAny<IDataParameter[]>()), Times.Never);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
        }

        [Fact]
        public void Test_AbsoluteExpiration()
        {
            options = Options.Create(new InternalCacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableInMemorySlidingExpiration = false,
                ExpirationInSeconds = 10
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new InternalCacheRepository(database.Object, memoryCache, options, logger.Object);

            repository[commandText, parameters] = "Sample Data Cached";

            database.Verify(db => db.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IDataParameter[]>()), Times.Never);

            Thread.Sleep(8000);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
            Assert.True(repository.ContainsValue(commandText, parameters));

            Thread.Sleep(11000);

            Assert.Null(repository[commandText, parameters]);
            Assert.False(repository.ContainsValue(commandText, parameters));
        }

        [Fact]
        public void Test_SlidingExpiration()
        {
            options = Options.Create(new InternalCacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableInMemorySlidingExpiration = true,
                ExpirationInSeconds = 10
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new InternalCacheRepository(database.Object, memoryCache, options, logger.Object);

            repository[commandText, parameters] = "Sample Data Cached";

            database.Verify(db => db.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IDataParameter[]>()), Times.Never);

            Thread.Sleep(8000);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
            Assert.True(repository.ContainsValue(commandText, parameters));

            Thread.Sleep(8000);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
            Assert.True(repository.ContainsValue(commandText, parameters));

            Thread.Sleep(11000);

            Assert.Null(repository[commandText, parameters]);
            Assert.False(repository.ContainsValue(commandText, parameters));

        }

    }
}
