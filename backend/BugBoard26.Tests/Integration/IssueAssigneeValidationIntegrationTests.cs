using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueAssigneeValidationIntegrationTests : ApiIntegrationTest
{
    [Fact]
    public async Task Create_WithExplicitNullAssignee_PersistsUnassignedTodoIssue()
    {
        await AuthenticateAsync("USER");

        using var response = await CreateIssueAsync(null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(returned.Id);
        Assert.Null(returned.AssignedToId);
        Assert.Null(saved.AssignedToId);
        Assert.Equal(IssueStatus.Todo, saved.Status);
        Assert.Equal(UserId, saved.CreatedById);
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("ADMIN")]
    public async Task Create_WithActiveUserOrAdminAssignee_PersistsAssignment(string role)
    {
        var assigneeId = await GetUserIdAsync(role);
        await AuthenticateAsync("USER");

        using var response = await CreateIssueAsync(assigneeId);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(returned.Id);
        Assert.Equal(assigneeId, returned.AssignedToId);
        Assert.Equal(assigneeId, saved.AssignedToId);
        Assert.Equal(UserId, saved.CreatedById);
        Assert.Equal(IssueStatus.Todo, saved.Status);
        Assert.Equal(Email(role), returned.AssignedToEmail);
    }

    [Theory]
    [InlineData("MISSING")]
    [InlineData("INACTIVE")]
    [InlineData("READONLY")]
    public async Task Create_WithInvalidAssignee_ReturnsBadRequest_AndDatabaseIsUnchanged(string assignee)
    {
        var assigneeId = assignee == "MISSING" ? int.MaxValue : await GetUserIdAsync(assignee);
        await AuthenticateAsync("USER");
        var before = await SnapshotAsync();

        using var response = await CreateIssueAsync(assigneeId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors").GetProperty("AssignedToId");
        Assert.Contains(errors.EnumerateArray(), error =>
            error.GetString()!.Contains("assegnatario", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task Suggestion_ExcludesInactiveAndReadonlyUsers_AndCanBeUsedForCreation()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admins = await db.Users.Where(user => user.Role == UserRole.Admin).ToListAsync();
            foreach (var admin in admins)
            {
                admin.IsActive = false;
            }
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("USER");

        using var suggestionResponse = await Client.GetAsync("/api/issues/suggest-assignee");

        Assert.Equal(HttpStatusCode.OK, suggestionResponse.StatusCode);
        var suggestion = await suggestionResponse.Content.ReadFromJsonAsync<SuggestAssigneeResponse>();
        Assert.NotNull(suggestion);
        Assert.Equal(UserId, suggestion.UserId);
        using var creationResponse = await CreateIssueAsync(suggestion.UserId);
        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);
        var returned = await ReadIssueFromResponseAsync(creationResponse);
        Assert.Equal(suggestion.UserId, returned.AssignedToId);
        Assert.Equal(suggestion.UserId, (await ReadIssueFromDatabaseAsync(returned.Id)).AssignedToId);
    }

    private Task<HttpResponseMessage> CreateIssueAsync(int? assigneeId)
    {
        return Client.PostAsJsonAsync("/api/issues", new
        {
            title = "Issue con assegnatario validato",
            description = "Verifica HTTP della validazione dell'assegnatario.",
            type = "Bug",
            assignedToId = assigneeId
        });
    }

    private async Task<int> GetUserIdAsync(string role)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Users.Where(user => user.Email == Email(role)).Select(user => user.Id).SingleAsync();
    }
}
