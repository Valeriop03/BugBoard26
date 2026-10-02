using System.Net;
using System.Net.Http.Json;
using BugBoard26.Api.Models;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssuePermissionsIntegrationTests : ApiIntegrationTest
{
    [Fact]
    public async Task Admin_CanArchiveIssue_WhichLeavesMainListButRemainsInArchiveAndDetail()
    {
        await AuthenticateAsync("ADMIN");
        Assert.Contains(await ReadIssuesAsync("/api/issues"), issue => issue.Id == IssueId);
        var before = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/archive", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await ReadIssueFromResponseAsync(response)).IsArchived);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.True(saved.IsArchived);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        Assert.DoesNotContain(await ReadIssuesAsync("/api/issues"), issue => issue.Id == IssueId);
        Assert.Contains(await ReadIssuesAsync("/api/issues/archived"), issue => issue.Id == IssueId && issue.IsArchived);
        using var detailResponse = await Client.GetAsync($"/api/issues/{IssueId}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.True((await ReadIssueFromResponseAsync(detailResponse)).IsArchived);
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    public async Task NonAdmin_CannotArchiveIssue_AndDatabaseIsUnchanged(string role)
    {
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/archive", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task Admin_CanMarkIssueAsDuplicate_WithPersistedStatusAndOriginalReference()
    {
        await AuthenticateAsync("ADMIN");
        var before = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new
        {
            originalIssueId = OriginalIssueId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var duplicate = await ReadIssueFromResponseAsync(response);
        Assert.Equal(IssueStatus.Duplicate, duplicate.Status);
        Assert.Equal(OriginalIssueId, duplicate.DuplicateOfIssueId);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(IssueStatus.Duplicate, saved.Status);
        Assert.Equal(OriginalIssueId, saved.DuplicateOfIssueId);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        var original = await ReadIssueFromDatabaseAsync(OriginalIssueId);
        Assert.Equal(IssueStatus.Todo, original.Status);
        Assert.Null(original.DuplicateOfIssueId);
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    public async Task NonAdmin_CannotMarkIssueAsDuplicate_AndDatabaseIsUnchanged(string role)
    {
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new
        {
            originalIssueId = OriginalIssueId
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Admin_CannotUseSelfOrMissingOriginal_AndDatabaseIsUnchanged(bool selfReference)
    {
        await AuthenticateAsync("ADMIN");
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new
        {
            originalIssueId = selfReference ? IssueId : int.MaxValue
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("login")]
    [InlineData("api")]
    public async Task Readonly_CanSearchCaseInsensitiveTitleOrDescription_WithPostgreSqlILike(string keyword)
    {
        await AuthenticateAsync("READONLY");

        var issues = await ReadIssuesAsync($"/api/issues?keyword={keyword}");

        Assert.Equal(IssueId, Assert.Single(issues).Id);
    }
}
