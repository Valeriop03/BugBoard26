using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Data;
using BugBoard26.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class UsersIntegrationTests : ApiIntegrationTest
{
    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    public async Task Admin_CanCreateUser_WithoutExposingPasswordHash(string role)
    {
        await AuthenticateAsync("ADMIN");
        const string email = "new-user@integration.bugboard26.local";

        using var response = await Client.PostAsJsonAsync("/api/users", new { email, password = Password, role });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.False(body.Contains("password", StringComparison.OrdinalIgnoreCase));
        using var document = JsonDocument.Parse(body);
        Assert.Equal(email, document.RootElement.GetProperty("email").GetString());
        Assert.Equal(role, document.RootElement.GetProperty("role").GetString());

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.AsNoTracking().SingleAsync(user => user.Email == email);
            Assert.Equal(document.RootElement.GetProperty("id").GetInt32(), user.Id);
            Assert.Equal(role, UserRoleMapper.ToApiRole(user.Role));
            Assert.True(user.IsActive);
            Assert.NotEqual(Password, user.PasswordHash);
            Assert.True(scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Verify(Password, user.PasswordHash));
        }

        using var listResponse = await Client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.False(listBody.Contains("password", StringComparison.OrdinalIgnoreCase));
        using var list = JsonDocument.Parse(listBody);
        Assert.Contains(list.RootElement.EnumerateArray(), item => item.GetProperty("email").GetString() == email);

        using var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.False((await loginResponse.Content.ReadAsStringAsync()).Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    public async Task NonAdmin_CannotCreateUser_AndDatabaseIsUnchanged(string role)
    {
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PostAsJsonAsync("/api/users", new
        {
            email = "forbidden-user@integration.bugboard26.local",
            password = Password,
            role = "USER"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }
}
