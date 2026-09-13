using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Seeders;

namespace TaskManagement.Api.Tests.Integration;

public sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string? _maintenanceConnectionString;
    private string? _testConnectionString;
    private string? _databaseName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddUserSecrets<Program>(optional: true);
            configuration.AddEnvironmentVariables();
        });
        builder.ConfigureServices((context, services) =>
        {
            var sourceConnectionString = context.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Default database connection is not configured");

            var testDatabaseName = $"task_management_test_{Guid.NewGuid():N}";
            var maintenanceBuilder = new NpgsqlConnectionStringBuilder(sourceConnectionString)
            {
                Database = "postgres"
            };
            var testBuilder = new NpgsqlConnectionStringBuilder(sourceConnectionString)
            {
                Database = testDatabaseName
            };

            _databaseName = testDatabaseName;
            _maintenanceConnectionString = maintenanceBuilder.ConnectionString;
            _testConnectionString = testBuilder.ConnectionString;

            CreateTestDatabase();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_testConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        _ = CreateClient();

        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    public async Task RefreshDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.RefreshAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();

        if (_maintenanceConnectionString is null || _databaseName is null)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();

        await using var terminateCommand = new NpgsqlCommand(
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @databaseName AND pid <> pg_backend_pid();",
            connection);
        terminateCommand.Parameters.AddWithValue("databaseName", _databaseName);
        await terminateCommand.ExecuteNonQueryAsync();

        var quotedName = new NpgsqlCommandBuilder().QuoteIdentifier(_databaseName);
        await using var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS {quotedName};", connection);
        await dropCommand.ExecuteNonQueryAsync();
    }

    private void CreateTestDatabase()
    {
        using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        connection.Open();

        var quotedName = new NpgsqlCommandBuilder().QuoteIdentifier(_databaseName!);
        using var command = new NpgsqlCommand($"CREATE DATABASE {quotedName};", connection);
        command.ExecuteNonQuery();
    }
}
