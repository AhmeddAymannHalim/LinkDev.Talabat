using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace LinkDev.Talabat.Tests.Integration
{
    /// <summary>
    /// Boots the real API against throw-away SQL Server databases (migrated and seeded by the app
    /// itself) and a real Redis server.
    ///
    /// Configure with environment variables:
    ///   TALABAT_TEST_SQL    SQL Server connection string without a Database
    ///                       (default: local SQL Server Express)
    ///   TALABAT_TEST_REDIS  Redis host:port (default: localhost:6379)
    ///
    /// Tests are skipped, not failed, when either server is unreachable.
    /// </summary>
    public sealed class TalabatApiFactory : WebApplicationFactory<LinkDev.Talabat.APIs.Program>
    {
        private const string DefaultSql = @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";
        private const string DefaultRedis = "localhost:6379";

        private readonly string _sqlServer = Environment.GetEnvironmentVariable("TALABAT_TEST_SQL") ?? DefaultSql;
        private readonly string _redis = Environment.GetEnvironmentVariable("TALABAT_TEST_REDIS") ?? DefaultRedis;
        private readonly string _suffix = Guid.NewGuid().ToString("N")[..10];

        public bool SqlAvailable { get; }
        public bool RedisAvailable { get; }
        public bool Ready => SqlAvailable && RedisAvailable;

        public string SkipReason =>
            !SqlAvailable ? "SQL Server is not reachable (set TALABAT_TEST_SQL)."
            : !RedisAvailable ? "Redis is not reachable (set TALABAT_TEST_REDIS)."
            : "";

        public TalabatApiFactory()
        {
            SqlAvailable = CanConnectToSql();
            RedisAvailable = CanConnectToRedis();
            if (!Ready) return;

            // Environment variables are read by WebApplication.CreateBuilder, so they win over appsettings.
            Environment.SetEnvironmentVariable("ConnectionStrings__StoreContext", DatabaseConnection("Store"));
            Environment.SetEnvironmentVariable("ConnectionStrings__IdentityContext", DatabaseConnection("Identity"));
            Environment.SetEnvironmentVariable("ConnectionStrings__Redis", _redis);
            Environment.SetEnvironmentVariable("JwtSettings__Key", "integration-test-signing-key-that-is-long-enough-0123456789");
        }

        private string DatabaseName(string kind) => $"Talabat.IT.{kind}.{_suffix}";

        private string DatabaseConnection(string kind) =>
            new SqlConnectionStringBuilder(_sqlServer) { InitialCatalog = DatabaseName(kind) }.ConnectionString;

        private bool CanConnectToSql()
        {
            try
            {
                using var connection = new SqlConnection(_sqlServer);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CanConnectToRedis()
        {
            try
            {
                var parts = _redis.Split(':');
                var port = parts.Length > 1 ? int.Parse(parts[1]) : 6379;
                using var client = new TcpClient();
                return client.ConnectAsync(parts[0], port).Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing || !SqlAvailable) return;

            foreach (var kind in new[] { "Store", "Identity" })
            {
                try
                {
                    var name = DatabaseName(kind);
                    using var connection = new SqlConnection(_sqlServer);
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText =
                        $"IF DB_ID(N'{name}') IS NOT NULL BEGIN " +
                        $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]; END";
                    command.ExecuteNonQuery();
                }
                catch
                {
                    // Best effort: a leftover test database is harmless.
                }
            }
        }
    }
}
