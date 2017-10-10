using Yc.Sql.Entity.Data.Core.Framework.Access;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Yc.Sql.Entity.Data.Core.Framework.Cache
{
    public class CacheRepository : ICacheRepository
    {
        private readonly StringBuilder keyStringBuilder = new StringBuilder();
        private readonly UnicodeEncoding encoding = new UnicodeEncoding();
        private readonly SHA1Managed shaManaged = new SHA1Managed();
        protected Dictionary<string, string> HashKeyCache = new Dictionary<string, string>();
        protected Dictionary<string, DateTime> currentTimeStamps = new Dictionary<string, DateTime>();
        protected readonly IDatabase database;
        private IDistributedCache cache;
        private DistributedCacheEntryOptions cacheEntryOptions;
        private IOptions<CacheConfiguration> cacheOptions;
        private ILogger<CacheRepository> logger;

        public CacheRepository(IDatabase database, IDistributedCache cache, IOptions<CacheConfiguration> cacheOptions, ILogger<CacheRepository> logger)
        {
            this.database = database;
            this.cache = cache;
            this.cacheOptions = cacheOptions;
            this.logger = logger;

            cacheEntryOptions = new DistributedCacheEntryOptions();

            if (cacheOptions.Value.EnableSlidingExpiration)
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(cacheOptions.Value.ExpirationInSeconds));
            else 
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheOptions.Value.ExpirationInSeconds));

            logger.LogDebug($"CacheConfiguration:{cacheOptions.Value}");
        }

        protected string GetKey(string commandText, IEnumerable<IDataParameter> parameterCollection)
        {
            keyStringBuilder.Length = 0;
            keyStringBuilder.AppendFormat("{0}-", commandText);
            foreach (var parameter in parameterCollection)
            {
                keyStringBuilder.AppendFormat("{0}-{1}-", parameter.ParameterName, (parameter.Value ?? "null"));
            }

            var stringKey = keyStringBuilder.ToString();
            string hashKey;

            if (!HashKeyCache.TryGetValue(stringKey, out hashKey))
            {
                hashKey = GetHashCode(stringKey);
                HashKeyCache[stringKey] = hashKey;
                logger.LogDebug($"Cache Key created:{hashKey}");
            }
            return hashKey;
        }

        protected string GetHashCode(string key)
        {
            return BitConverter.ToString(shaManaged.ComputeHash(encoding.GetBytes(key)));
        }

        protected DateTime DependentTablesLatestModifiedTimeStamp(string commandText, string dependantTableNamesCsv)
        {
            object latestModifiedTimeStamp = null;
            latestModifiedTimeStamp = database.ExecuteScalar(CommandType.Text, string.Format(getModifiedTimestampQuery, dependantTableNamesCsv), 0, new IDataParameter[0]);
            return Convert.IsDBNull(latestModifiedTimeStamp) ? DateTime.MinValue : Convert.ToDateTime(latestModifiedTimeStamp);
        }

        protected bool ContainsValue(string key, out byte[] value, string commandText, string dependantTableNamesCsv)
        {
            value = cache.Get(key);

            var isFound = value != null;

            if (cacheOptions.Value.EnableDatabaseChangeRefresh
                && isFound
                && database != null
                && getModifiedTimestampQuery != null
                && dependantTableNamesCsv != null)
            {
                DateTime timestamp;
                if (currentTimeStamps.TryGetValue(key, out timestamp))
                {
                    var dbTimeStamp = DependentTablesLatestModifiedTimeStamp(commandText, dependantTableNamesCsv);
                    if (dbTimeStamp > timestamp)
                        isFound = false;
                }
                else
                    isFound = false;
            }

            logger.LogDebug($"Cache data found: {isFound}, key:{key}");

            return isFound && value != null;
        }

        public byte[] this[string commandText, string dependantTableNamesCsv, IEnumerable<IDataParameter> parameterCollection]
        {
            get
            {
                byte[] value;
                var key = GetKey(commandText, parameterCollection);
                logger.LogDebug($"Retrieving data from cache, key:{key}");
                ContainsValue(key, out value, commandText, dependantTableNamesCsv);
                return value;
            }
            set
            {
                var key = GetKey(commandText, parameterCollection);
                logger.LogDebug($"Setting data to cache, key:{key}");
                cache.Set(key, value, cacheEntryOptions);
                if (cacheOptions.Value.EnableDatabaseChangeRefresh)
                    currentTimeStamps[key] = DateTime.Now;
            }
        }

        public byte[] this[string commandText, IEnumerable<IDataParameter> parameterCollection]
        {
            get
            {
                return this[commandText, null, parameterCollection];
            }
            set
            {
                this[commandText, null, parameterCollection] = value;
            }
        }

        public bool ContainsValue(string commandText, string dependantTableNamesCsv, IEnumerable<IDataParameter> parameterCollection)
        {
            byte[] dummyValue;
            var key = GetKey(commandText, parameterCollection);
            return ContainsValue(key, out dummyValue, commandText, dependantTableNamesCsv);
        }

        public bool ContainsValue(string commandText, IEnumerable<IDataParameter> parameterCollection)
        {
            return ContainsValue(commandText, null, parameterCollection);
        }

        protected string getModifiedTimestampQuery = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.CsvToTable') AND OBJECTPROPERTY(object_id, N'IsTableFunction')=1)
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
                                                        END
                                                        GO
                                                        WITH TABLENAMES(NAME) AS 
                                                        (
	                                                        SELECT NAME = VALUE FROM dbo.CSVTOTABLE('DataTransaction,GeneralCode')
                                                        ) 
                                                        SELECT MAX(ISNULL(LAST_USER_UPDATE,
                                                        CONVERT(DATETIME,'1/1/1901'))) FROM sys.dm_db_index_usage_stats S 
                                                        RIGHT OUTER JOIN TABLENAMES T ON S.OBJECT_ID=OBJECT_ID(T.NAME) 
                                                        WHERE DATABASE_ID = DB_ID();";
    }
}
