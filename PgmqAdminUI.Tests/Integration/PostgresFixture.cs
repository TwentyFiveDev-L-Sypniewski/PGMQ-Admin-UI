using Npgsql;
using Respawn;
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
    private static readonly RespawnerOptions s_respawnerOptions = new()
    {
        DbAdapter = DbAdapter.Postgres,
        SchemasToInclude = ["pgmq"],
        TablesToIgnore = ["meta"] // Keep PGMQ metadata table
    };

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

    /// <summary>
    /// Resets the database to a clean state by deleting all data from PGMQ tables.
    /// Call this after each test to ensure test isolation.
    /// Respawner is recreated on each call because PGMQ creates tables dynamically.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();

        // Check if any PGMQ tables exist (other than meta) before attempting reset
        await using var checkCommand = new NpgsqlCommand(
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'pgmq' AND table_name != 'meta');",
            connection);
        var hasTablesResult = await checkCommand.ExecuteScalarAsync();
        var hasTables = hasTablesResult is true;

        if (!hasTables)
        {
            return; // No tables to reset
        }

        // Create Respawner dynamically because PGMQ creates tables on-the-fly
        var respawner = await Respawner.CreateAsync(connection, s_respawnerOptions);
        await respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
