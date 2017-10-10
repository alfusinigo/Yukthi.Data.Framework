using System.Text;

namespace Yc.Sql.Entity.Data.Core.Framework.Cache
{
    public class DatabaseConfiguration
    {
        public string SqlConnectionString { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"SqlConnectionString: {SqlConnectionString}");
            return builder.ToString();
        }
    }
}
