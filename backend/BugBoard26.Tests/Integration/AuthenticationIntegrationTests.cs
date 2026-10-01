using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BugBoard26.Api.Models;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class AuthenticationIntegrationTests : ApiIntegrationTest
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsUsableJwt()
    {
        await AuthenticateAsync("USER");

        using var response = await Client.GetAsync("/api/issues");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("USER", "WrongPassword123!")]
    [InlineData("INACTIVE", Password)]
    public async Task Login_WrongPasswordOrInactiveUser_ReturnsUnauthorizedWithoutChanges(string role, string password)
    {
        var before = await SnapshotAsync();

        using var response = await Client.PostAsJsonAsync("/api/auth/login", new { email = Email(role), password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False((await response.Content.ReadAsStringAsync()).Contains("\"token\"", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetIssues_MissingOrInvalidToken_ReturnsUnauthorizedWithoutChanges(bool invalidToken)
    {
        if (invalidToken)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
        }
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task Readonly_CanReadIssueListAndDetail()
    {
        await AuthenticateAsync("READONLY");

        var issues = await ReadIssuesAsync("/api/issues");
        using var response = await Client.GetAsync($"/api/issues/{IssueId}");

        Assert.Contains(issues, issue => issue.Id == IssueId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await ReadIssueFromResponseAsync(response);
        Assert.Equal(IssueId, detail.Id);
        Assert.Equal("Errore LOGIN", detail.Title);
    }

    [Fact]
    public async Task Readonly_CannotCreateIssue_AndDatabaseIsUnchanged()
    {
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.PostAsJsonAsync("/api/issues", new
        {
            title = "Nuova issue vietata",
            description = "Descrizione valida per la richiesta.",
            type = "Bug"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task User_CanCreateIssue_WithAuthenticatedCreatorAndTodoStatus()
    {
        await AuthenticateAsync("USER");

        using var response = await Client.PostAsJsonAsync("/api/issues", new
        {
            title = "Nuova issue reale",
            description = "Descrizione della issue creata tramite HTTP.",
            type = "Bug"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(created.Id);
        Assert.Equal(UserId, created.CreatedById);
        Assert.Equal(UserId, saved.CreatedById);
        Assert.Equal(IssueStatus.Todo, created.Status);
        Assert.Equal(IssueStatus.Todo, saved.Status);
        Assert.Null(saved.AssignedToId);
        Assert.Equal("Nuova issue reale", saved.Title);
        Assert.NotNull(response.Headers.Location);
        using var detail = await Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }
}
