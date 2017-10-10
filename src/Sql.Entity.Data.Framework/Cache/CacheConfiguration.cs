using System.Text;

namespace Yc.Sql.Entity.Data.Core.Framework.Cache
{
    public class CacheConfiguration
    {
        //Default 30 Minutes
        public int ExpirationInSeconds { get; set; } = 1800;
        //Default to true
        public bool EnableSlidingExpiration { get; set; } = true;

        public bool EnableDatabaseChangeRefresh { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"EnableDatabaseChangeRefresh: {EnableDatabaseChangeRefresh}");
            builder.AppendLine($"ExpirationInSeconds: {ExpirationInSeconds}");
            builder.AppendLine($"EnableSlidingExpiration: {EnableSlidingExpiration}");
            return builder.ToString();
        }
    }
}
