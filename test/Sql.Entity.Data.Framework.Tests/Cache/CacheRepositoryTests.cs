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
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using System.Text;

namespace Sql.Entity.Data.Core.Framework.Tests.Cache
{
    public class CacheRepositoryTests
    {
        IOptions<CacheConfiguration> cacheOptions;
        IOptions<MemoryDistributedCacheOptions> distributedCacheOptions;
        Mock<ILogger<CacheRepository>> logger;
        IDistributedCache memoryCache;
        IDistributedCache externalCache;
        Mock<IDatabase> database;
        ICacheRepository repository;
        string getModifiedTimestampQuery = @"WITH TABLENAMES(NAME) AS (
                                                    SELECT NAME = VALUE FROM dbo.CSVTOTABLE('{0}')) 
                                                    SELECT MAX(ISNULL(LAST_USER_UPDATE,
                                                    CONVERT(DATETIME,'1/1/1901'))) FROM sys.dm_db_index_usage_stats S 
                                                    RIGHT OUTER JOIN TABLENAMES T ON S.OBJECT_ID=OBJECT_ID(T.NAME) 
                                                    WHERE DATABASE_ID = DB_ID();";

        public CacheRepositoryTests()
        {
            logger = new Mock<ILogger<CacheRepository>>();
            distributedCacheOptions = Options.Create(new MemoryDistributedCacheOptions());
            memoryCache = new MemoryDistributedCache(distributedCacheOptions);

            externalCache = new ExternalTestCache();
            database = new Mock<IDatabase>();
        }

        [Fact]
        public void Test_SetAndGetAndContainsCacheWithoutDbDependency_InMemory()
        {
            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableSlidingExpiration = true,
                ExpirationInSeconds = 60
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new CacheRepository(database.Object, memoryCache, cacheOptions, logger.Object);

            Assert.False(repository.ContainsValue(commandText, parameters));

            repository[commandText, parameters] = "Sample Data Cached";

            Assert.True(repository.ContainsValue(commandText, parameters));

            database.Verify(db => db.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IDataParameter[]>()), Times.Never);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
        }

        [Fact]
        public void Test_SetAndGetAndContainsCacheWithoutDbDependency_External()
        {
            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableSlidingExpiration = true,
                ExpirationInSeconds = 60
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new CacheRepository(database.Object, memoryCache, cacheOptions, logger.Object);

            Assert.False(repository.ContainsValue(commandText, parameters));

            repository[commandText, parameters] = "Sample Data Cached";

            Assert.True(repository.ContainsValue(commandText, parameters));

            database.Verify(db => db.ExecuteScalar(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IDataParameter[]>()), Times.Never);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
        }

        [Fact]
        public void Test_SetAndGetAndContainsCacheWithDbDependency_InMemory()
        {
            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = true,
                EnableSlidingExpiration = true,
                ExpirationInSeconds = 60
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new CacheRepository(database.Object, memoryCache, cacheOptions, logger.Object);

            Assert.False(repository.ContainsValue(commandText, "table1,table2", parameters));

            repository[commandText, "table1,table2", parameters] = "Sample Data Cached";

            Assert.True(repository.ContainsValue(commandText, "table1,table2", parameters));

            database.Verify(db => db.ExecuteScalar(It.Is<CommandType>(val => val == CommandType.Text), It.Is<string>(val => val == string.Format(getModifiedTimestampQuery, "table1,table2")), It.Is<int>(val => val == 0), It.IsAny<IDataParameter[]>()), Times.Never);

            Assert.Equal(repository[commandText, parameters], "Sample Data Cached");
        }

        [Fact]
        public void Test_AbsoluteExpiration_InMemory()
        {
            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableSlidingExpiration = false,
                ExpirationInSeconds = 10
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new CacheRepository(database.Object, memoryCache, cacheOptions, logger.Object);

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
        public void Test_SlidingExpiration_InMemory()
        {
            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableSlidingExpiration = true,
                ExpirationInSeconds = 10
            });

            const string commandText = "text";
            var parameters = new SqlParameter[] { new SqlParameter("param1", "param1Value") };

            repository = new CacheRepository(database.Object, memoryCache, cacheOptions, logger.Object);

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

        [Fact]
        public void TestGetModifiedTimestampQuery()
        {
            var expectedGetModifiedTimestampQuery = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.CsvToTable') AND OBJECTPROPERTY(object_id, N'IsTableFunction')=1)
                                                        BEGIN
                                                            EXEC( N'CREATE FUNCTION dbo.CsvToTable 
		                                                        (@CSV varchar(MAX))
		                                                        RETURNS @valueTable table (value varchar(256), rownum int)
		                                                        AS
		                                                        BEGIN
			                                                        if @CSV <> ''''
			                                                        BEGIN
				                                                        declare @seperator char(1)
				                                                        set @seperator = '',''
		
				                                                        declare @sep_position int
				                                                        declare @arr_val varchar(max)
				                                                        declare @rowcount int
				                                                        set @rowcount = 1
		
				                                                        if RIGHT(@csv,1) != '',''
					                                                        set @CSV = @CSV+'',''
			
				                                                        while PATINDEX(''%,%'',@csv) <> 0
				                                                        BEGIN
					                                                        select @sep_position = PATINDEX(''%,%'', @csv)
					                                                        select @arr_val = LEFT(@csv, @sep_position - 1)
					                                                        insert @valueTable values (ltrim(rtrim(@arr_val)), @rowcount)
					                                                        select @CSV=STUFF(@csv,1,@sep_position,'''')
					                                                        set @rowcount = @rowcount + 1
				                                                        END
			                                                        END
			                                                        RETURN
		                                                        END');
                                                        END;
                                                        
                                                        WITH TABLENAMES(NAME) AS 
                                                        (
	                                                        SELECT NAME = VALUE FROM dbo.CSVTOTABLE('DataTransaction,GeneralCode')
                                                        ) 
                                                        SELECT MAX(ISNULL(LAST_USER_UPDATE,
                                                        CONVERT(DATETIME,'1/1/1901'))) FROM sys.dm_db_index_usage_stats S 
                                                        RIGHT OUTER JOIN TABLENAMES T ON S.OBJECT_ID=OBJECT_ID(T.NAME) 
                                                        WHERE DATABASE_ID = DB_ID();";

            cacheOptions = Options.Create(new CacheConfiguration
            {
                EnableDatabaseChangeRefresh = false,
                EnableSlidingExpiration = false,
                ExpirationInSeconds = 10
            });

            var repositoryStub = new CacheRepositoryStub(database.Object, memoryCache, cacheOptions, logger.Object);

            Assert.Equal(repositoryStub.GetModifiedTimestampQuery, expectedGetModifiedTimestampQuery);
        }

    }

    internal class CacheRepositoryStub : CacheRepository
    {
        public CacheRepositoryStub(IDatabase database, IDistributedCache cache, IOptions<CacheConfiguration> cacheOptions, ILogger<CacheRepository> logger) : base(database, cache, cacheOptions, logger)
        {
        }

        public string GetModifiedTimestampQuery { get { return base.getModifiedTimestampQuery; } }
    }

    internal class ExternalTestCache : IDistributedCache
    {
        private Dictionary<string, byte[]> container = new Dictionary<string, byte[]>();

        public byte[] Get(string key)
        {
            byte[] value;

            if (container.TryGetValue(key, out value))
                return value;

            return null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            container.Add(key, value);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public void Refresh(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public void Remove(string key)
        {
            container.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }
    }
}
