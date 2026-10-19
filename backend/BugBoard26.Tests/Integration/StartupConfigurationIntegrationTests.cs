using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BugBoard26.Api.Data;
using BugBoard26.Api.Dtos.Auth;
using BugBoard26.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class StartupConfigurationIntegrationTests
{
    [Theory]
    [InlineData("ConnectionStrings:DefaultConnection", null)]
    [InlineData("ConnectionStrings:DefaultConnection", " ")]
    [InlineData("Jwt:Key", null)]
    [InlineData("Jwt:Key", " ")]
    [InlineData("Jwt:Key", "too-short")]
    [InlineData("DefaultAdmin:Email", null)]
    [InlineData("DefaultAdmin:Email", " ")]
    [InlineData("DefaultAdmin:Password", null)]
    [InlineData("DefaultAdmin:Password", " ")]
    public async Task MissingOrInvalidConfiguration_FailsClearly(string key, string? value)
    {
        var factory = new IssueApiFactory(new Dictionary<string, string?> { [key] = value });
        await factory.CreateDatabaseAsync();
        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains(key.StartsWith("DefaultAdmin:", StringComparison.Ordinal) ? "DefaultAdmin" : key, exception.Message);
        }
        finally
        {
            await factory.DisposeAsync();
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ConfiguredBootstrapAdmin_CanLogin_AndUseAuthenticatedApi()
    {
        var factory = new IssueApiFactory(new Dictionary<string, string?>
        {
            ["DefaultAdmin:Email"] = " BOOTSTRAP@integration.bugboard26.local "
        });
        await factory.CreateDatabaseAsync();
        try
        {
            using var client = factory.CreateClient();
            var configuration = factory.Services.GetRequiredService<IConfiguration>();

            using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "bootstrap@integration.bugboard26.local",
                password = configuration["DefaultAdmin:Password"]
            });

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(login);
            Assert.Equal("ADMIN", login.User.Role);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
            using var usersResponse = await client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
            Assert.DoesNotContain("password", await usersResponse.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await factory.DisposeAsync();
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ExistingAdmin_IsNotChanged_WhenBootstrapCredentialsAreMissing()
    {
        var factory = new IssueApiFactory();
        await factory.CreateDatabaseAsync();
        try
        {
            using var client = factory.CreateClient();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var before = await db.Users.AsNoTracking().SingleAsync();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration["DefaultAdmin:Email"] = null;
            configuration["DefaultAdmin:Password"] = null;

            await DatabaseSeeder.SeedAdminAsync(factory.Services);

            var after = await db.Users.AsNoTracking().SingleAsync();
            Assert.Equal(before.Id, after.Id);
            Assert.Equal(before.Email, after.Email);
            Assert.Equal(before.PasswordHash, after.PasswordHash);
            Assert.Equal(before.Role, after.Role);
            Assert.Equal(before.IsActive, after.IsActive);
            Assert.Equal(before.CreatedAt, after.CreatedAt);
            using var response = await client.GetAsync("/api/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await factory.DisposeAsync();
            await factory.DeleteDatabaseAsync();
        }
    }
}
