using BugBoard26.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace BugBoard26.Tests.Integration;

public class IssueApiFactory : WebApplicationFactory<Program>
{
    private readonly string _baseConnectionString;
    private readonly string _databaseName = $"bugboard26_test_{Guid.NewGuid():N}";
    private readonly string _testConnectionString;
    private bool _databaseCreated;

    public IssueApiFactory()
    {
        var connection = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("BUGBOARD26_TEST_CONNECTION_STRING")
            ?? "Host=127.0.0.1;Port=55432;Database=bugboard26_tests;Username=bugboard26_tests;Password=bugboard26_tests")
        {
            Pooling = false,
            Timeout = 5,
            CommandTimeout = 15
        };

        if (connection.Database != "bugboard26_tests")
        {
            throw new InvalidOperationException(
                "Il database di partenza dei test deve chiamarsi bugboard26_tests. Non usare il database di sviluppo.");
        }

        _baseConnectionString = connection.ConnectionString;
        connection.Database = _databaseName;
        _testConnectionString = connection.ConnectionString;
    }

    public async Task CreateDatabaseAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_baseConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
            _databaseCreated = true;
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException)
        {
            throw new InvalidOperationException(
                "PostgreSQL di test non disponibile o utente senza CREATEDB. Avvia il Compose nella cartella BugBoard26.Tests; vedi README.md.",
                exception);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _testConnectionString
            }));

        builder.ConfigureServices(services =>
        {
            // Replace every EF configuration before the startup seeder runs.
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_testConnectionString));
        });
    }

    public async Task DeleteDatabaseAsync()
    {
        if (!_databaseCreated)
        {
            return;
        }

        // Only the database generated and created by this factory can be removed.
        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE \"{_databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
        _databaseCreated = false;
    }
}
