using Npgsql;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace PgmqAdminUI.Tests.Integration;

/// <summary>
/// Shared PostgreSQL container fixture with PGMQ extension for integration tests.
/// Use with [ClassDataSource&lt;PostgresFixture&gt;(Shared = SharedType.Keyed, Key = "SharedDatabase")]
/// to share a single container across multiple test classes.
/// </summary>
public sealed class PostgresFixture : IAsyncInitializer, IAsyncDisposable
{
    private PostgreSqlContainer? _container;

    public string PostgresConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("ghcr.io/pgmq/pg18-pgmq:v1.7.0")
            .WithName($"pgmq-postgres-shared-integration-tests-{Guid.NewGuid():N}")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _container.StartAsync();

        PostgresConnectionString = _container.GetConnectionString();

        // Enable PGMQ extension
        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS pgmq CASCADE;", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
