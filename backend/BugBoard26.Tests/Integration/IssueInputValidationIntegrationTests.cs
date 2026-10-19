using System.Net;
using System.Text;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueInputValidationIntegrationTests : ApiIntegrationTest
{
    [Theory]
    [InlineData("{\"title\":\"Tipo non valido\",\"description\":\"Descrizione valida\",\"type\":999}")]
    [InlineData("{\"title\":\"Tipo non valido\",\"description\":\"Descrizione valida\",\"type\":-1}")]
    [InlineData("{\"title\":\"Priorita non valida\",\"description\":\"Descrizione valida\",\"type\":\"Bug\",\"priority\":999}")]
    [InlineData("{\"title\":\"Priorita non valida\",\"description\":\"Descrizione valida\",\"type\":\"Bug\",\"priority\":-1}")]
    public async Task UndefinedTypeOrPriority_IsRejectedWithoutChanges(string json)
    {
        await AuthenticateAsync("USER");
        var before = await SnapshotAsync();
        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await Client.PostAsync("/api/issues", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    public async Task UndefinedStatus_IsRejectedWithoutChanges(int status)
    {
        await AuthenticateAsync("USER");
        var before = await SnapshotAsync();
        using var body = new StringContent($"{{\"status\":{status}}}", Encoding.UTF8, "application/json");

        using var response = await Client.PatchAsync($"/api/issues/{IssueId}/status", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }
}
