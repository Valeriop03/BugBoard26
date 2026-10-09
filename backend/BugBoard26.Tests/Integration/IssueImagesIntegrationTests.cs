using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueImagesIntegrationTests : ApiIntegrationTest
{
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXY/jPwPAfAAUAAf+mXJtdAAAAAElFTkSuQmCC");
    private static readonly byte[] Jpeg = Convert.FromBase64String(
        "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD50ooor8MP9Uz/2Q==");

    [Theory]
    [InlineData("USER", "photo.png", false)]
    [InlineData("ADMIN", "photo.jpg", true)]
    [InlineData("USER", "photo.jpeg", true)]
    [InlineData("USER", "PHOTO.PNG", false)]
    [InlineData("ADMIN", "../../escape.png", false)]
    public async Task CreatorOrAdmin_UploadsAndReadsPngOrJpeg_WithoutTrustingMimeOrOriginalName(string role, string name, bool jpeg)
    {
        await AuthenticateAsync(role);
        var bytes = jpeg ? Jpeg : Png;

        using var response = await UploadAsync(Client, IssueId, bytes, name, "application/octet-stream");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        Assert.True(returned.HasImage);
        var body = await response.Content.ReadAsStringAsync();
        Assert.False(body.Contains("imagePath", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(Factory.ImageDirectory, body);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.NotNull(saved.ImagePath);
        Assert.DoesNotContain("escape", saved.ImagePath);
        Assert.DoesNotContain('/', saved.ImagePath);
        Assert.DoesNotContain('\\', saved.ImagePath);
        Assert.True(Guid.TryParseExact(Path.GetFileNameWithoutExtension(saved.ImagePath), "N", out _));
        Assert.Equal(bytes, await File.ReadAllBytesAsync(Assert.Single(Directory.GetFiles(Factory.ImageDirectory))));
        using var read = await Client.GetAsync($"/api/issues/{IssueId}/image");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(jpeg ? "image/jpeg" : "image/png", read.Content.Headers.ContentType!.MediaType);
        Assert.Equal(bytes, await read.Content.ReadAsByteArrayAsync());
        Assert.Null(read.Content.Headers.ContentDisposition);
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("ADMIN")]
    [InlineData("READONLY")]
    public async Task AnyAuthenticatedRole_CanReadArchivedImage(string role)
    {
        await UploadInitialImageAsync();
        await AuthenticateAsync("ADMIN");
        using var archive = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/archive", new { });
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        await AuthenticateAsync(role);

        using var response = await Client.GetAsync($"/api/issues/{IssueId}/image");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Png, await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("OTHER", false)]
    [InlineData("READONLY", false)]
    [InlineData("READONLY", true)]
    public async Task NonCreatorOrReadonly_CannotUpload_AndDataAndFilesAreUnchanged(string role, bool readonlyCreator)
    {
        await UploadInitialImageAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
            if (role == "OTHER")
            {
                var user = new User
                {
                    Email = Email("OTHER"), Role = UserRole.User,
                    PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(Password)
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                issue.AssignedToId = user.Id;
            }
            if (readonlyCreator)
            {
                issue.CreatedById = await db.Users.Where(user => user.Email == Email("READONLY")).Select(user => user.Id).SingleAsync();
            }
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await UploadAsync(Client, IssueId, Jpeg, "photo.jpg");

        await AssertUnchangedAsync(response, HttpStatusCode.Forbidden, before, files);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithoutJwt_CannotReadOrUpload_AndDataAndFilesAreUnchanged(bool upload)
    {
        await UploadInitialImageAsync();
        Client.DefaultRequestHeaders.Authorization = null;
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = upload
            ? await UploadAsync(Client, IssueId, Jpeg, "photo.jpg")
            : await Client.GetAsync($"/api/issues/{IssueId}/image");

        await AssertUnchangedAsync(response, HttpStatusCode.Unauthorized, before, files);
    }

    [Fact]
    public async Task CreatorChangedToReadonlyAfterLogin_CannotUpload()
    {
        await UploadInitialImageAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var creator = await db.Users.SingleAsync(user => user.Id == UserId);
            creator.Role = UserRole.Readonly;
            await db.SaveChangesAsync();
        }
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await UploadAsync(Client, IssueId, Jpeg, "photo.jpg");

        await AssertUnchangedAsync(response, HttpStatusCode.Forbidden, before, files);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("text")]
    [InlineData("extension")]
    [InlineData("png-as-jpeg")]
    [InlineData("jpeg-as-png")]
    [InlineData("truncated-png")]
    [InlineData("truncated-jpeg")]
    public async Task InvalidFile_ReturnsClearBadRequest_AndDataAndFilesAreUnchanged(string kind)
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();
        var bytes = kind switch
        {
            "empty" => Array.Empty<byte>(),
            "text" => Encoding.UTF8.GetBytes("Questo file non e' un'immagine."),
            "jpeg-as-png" => Jpeg,
            "truncated-png" => Png[..8],
            "truncated-jpeg" => Jpeg[..12],
            _ => Png
        };
        var name = kind == "extension" ? "photo.gif" :
            kind is "png-as-jpeg" or "truncated-jpeg" ? "photo.jpg" : "photo.png";

        using var response = await UploadAsync(Client, IssueId, bytes, name, "image/png");

        await AssertUnchangedAsync(response, HttpStatusCode.BadRequest, before, files);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrWrongField_ReturnsBadRequest_AndDataAndFilesAreUnchanged(bool wrongField)
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();
        using var form = new MultipartFormDataContent();
        if (wrongField)
        {
            form.Add(new ByteArrayContent(Png), "attachment", "photo.png");
        }

        using var response = await Client.PostAsync($"/api/issues/{IssueId}/image", form);

        await AssertUnchangedAsync(response, HttpStatusCode.BadRequest, before, files);
    }

    [Fact]
    public async Task NonMultipart_ReturnsBadRequest_AndDataAndFilesAreUnchanged()
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await Client.PostAsJsonAsync($"/api/issues/{IssueId}/image", new { image = "photo.png" });

        await AssertUnchangedAsync(response, HttpStatusCode.BadRequest, before, files);
    }

    [Fact]
    public async Task FileAboveLimit_ReturnsBadRequest_AndKeepsPreviousImage()
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await UploadAsync(Client, IssueId, PngWithTextChunk((int)IssueImageStorage.MaxImageBytes + 1), "large.png");

        await AssertUnchangedAsync(response, HttpStatusCode.BadRequest, before, files);
    }

    [Fact]
    public async Task FileExactlyAtLimit_IsAccepted()
    {
        await AuthenticateAsync("USER");
        var bytes = PngWithTextChunk((int)IssueImageStorage.MaxImageBytes);

        using var response = await UploadAsync(Client, IssueId, bytes, "large.png");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var read = await Client.GetAsync($"/api/issues/{IssueId}/image");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(bytes, await read.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task NoImage_ReturnsNotFound_AndHasImageIsFalseInJsonCreationAndReads()
    {
        await AuthenticateAsync("USER");
        using var create = await Client.PostAsJsonAsync("/api/issues", new
        {
            title = "Issue senza allegato", description = "Creazione JSON invariata.", type = "Bug"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.False((await ReadIssueFromResponseAsync(create)).HasImage);
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await Client.GetAsync($"/api/issues/{IssueId}/image");

        await AssertUnchangedAsync(response, HttpStatusCode.NotFound, before, files);
        using var detail = await Client.GetAsync($"/api/issues/{IssueId}");
        Assert.False((await ReadIssueFromResponseAsync(detail)).HasImage);
        Assert.All(await ReadIssuesAsync("/api/issues"), issue => Assert.False(issue.HasImage));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingIssue_ReturnsNotFound_AndDataAndFilesAreUnchanged(bool upload)
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = upload
            ? await UploadAsync(Client, int.MaxValue, Png, "photo.png")
            : await Client.GetAsync($"/api/issues/{int.MaxValue}/image");

        await AssertUnchangedAsync(response, HttpStatusCode.NotFound, before, files);
    }

    [Fact]
    public async Task MissingPhysicalFile_ReturnsNotFoundWithoutChangingData()
    {
        await UploadInitialImageAsync();
        File.Delete(Assert.Single(Directory.GetFiles(Factory.ImageDirectory)));
        var before = await SnapshotAsync();
        var files = FileSnapshot();

        using var response = await Client.GetAsync($"/api/issues/{IssueId}/image");

        await AssertUnchangedAsync(response, HttpStatusCode.NotFound, before, files);
    }

    [Fact]
    public async Task Replacement_RemovesOldFileAndPersistsNewImage()
    {
        await UploadInitialImageAsync();
        var before = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await UploadAsync(Client, IssueId, Jpeg, "replacement.jpeg");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.NotEqual(before.ImagePath, saved.ImagePath);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        Assert.False(File.Exists(Path.Combine(Factory.ImageDirectory, before.ImagePath!)));
        Assert.Equal(saved.ImagePath, Path.GetFileName(Assert.Single(Directory.GetFiles(Factory.ImageDirectory))));
        using var read = await Client.GetAsync($"/api/issues/{IssueId}/image");
        Assert.Equal("image/jpeg", read.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Jpeg, await read.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task DatabaseFailure_DeletesUnusedNewFileAndPreservesOldImageAndData()
    {
        await UploadInitialImageAsync();
        var before = await SnapshotAsync();
        var files = FileSnapshot();
        using var failingFactory = Factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddDbContext<ApplicationDbContext>(options => options.AddInterceptors(new FailImageSaveInterceptor()))));
        using var failingClient = failingFactory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        failingClient.DefaultRequestHeaders.Authorization = Client.DefaultRequestHeaders.Authorization;

        using var response = await UploadAsync(failingClient, IssueId, Jpeg, "replacement.jpg");

        await AssertUnchangedAsync(response, HttpStatusCode.InternalServerError, before, files);
        using var read = await Client.GetAsync($"/api/issues/{IssueId}/image");
        Assert.Equal(Png, await read.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task HasImage_IsIncludedInAllIssueProjections()
    {
        await UploadInitialImageAsync();
        using var detail = await Client.GetAsync($"/api/issues/{IssueId}");
        Assert.True((await ReadIssueFromResponseAsync(detail)).HasImage);
        Assert.True(Assert.Single(await ReadIssuesAsync("/api/issues"), issue => issue.Id == IssueId).HasImage);
        using var status = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "InProgress" });
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.True((await ReadIssueFromResponseAsync(status)).HasImage);
        await AuthenticateAsync("ADMIN");
        using var duplicate = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new { originalIssueId = OriginalIssueId });
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.True((await ReadIssueFromResponseAsync(duplicate)).HasImage);
        using var archive = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/archive", new { });
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        Assert.True((await ReadIssueFromResponseAsync(archive)).HasImage);
        Assert.True(Assert.Single(await ReadIssuesAsync("/api/issues/archived"), issue => issue.Id == IssueId).HasImage);
    }

    [Fact]
    public async Task ConcurrentReplacements_LeaveOnlyTheReferencedFile()
    {
        await UploadInitialImageAsync();

        var results = await Task.WhenAll(UploadAsync(Client, IssueId, Png, "one.png"), UploadAsync(Client, IssueId, Jpeg, "two.jpg"));

        foreach (var response in results)
        {
            using (response) { Assert.Equal(HttpStatusCode.OK, response.StatusCode); }
        }
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(saved.ImagePath, Path.GetFileName(Assert.Single(Directory.GetFiles(Factory.ImageDirectory))));
        using var read = await Client.GetAsync($"/api/issues/{IssueId}/image");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var bytes = await read.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.SequenceEqual(Png) || bytes.SequenceEqual(Jpeg));
    }

    [Fact]
    public async Task NewHostWithSameDatabaseAndDirectory_CanReadPersistedImage()
    {
        await UploadInitialImageAsync();
        using var secondFactory = Factory.WithWebHostBuilder(_ => { });
        using var secondClient = secondFactory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        secondClient.DefaultRequestHeaders.Authorization = Client.DefaultRequestHeaders.Authorization;

        using var read = await secondClient.GetAsync($"/api/issues/{IssueId}/image");

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(Png, await read.Content.ReadAsByteArrayAsync());
    }

    private async Task UploadInitialImageAsync()
    {
        await AuthenticateAsync("USER");
        using var response = await UploadAsync(Client, IssueId, Png, "initial.png");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, int id, byte[] data, string name, string mime = "image/png")
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(data);
        file.Headers.ContentType = new MediaTypeHeaderValue(mime);
        form.Add(file, "image", name);
        return await client.PostAsync($"/api/issues/{id}/image", form);
    }

    private async Task AssertUnchangedAsync(HttpResponseMessage response, HttpStatusCode status, string before, string files)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal(files, FileSnapshot());
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(Factory.ImageDirectory, body);
        if (status == HttpStatusCode.BadRequest)
        {
            using var json = JsonDocument.Parse(body);
            Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("message").GetString()));
        }
    }

    private string FileSnapshot()
    {
        if (!Directory.Exists(Factory.ImageDirectory)) { return "[]"; }
        return JsonSerializer.Serialize(Directory.GetFiles(Factory.ImageDirectory).OrderBy(path => path).Select(path => new
        {
            Name = Path.GetFileName(path), Hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
        }));
    }

    private static byte[] PngWithTextChunk(int size)
    {
        var result = new byte[size];
        var offset = Png.Length - 12;
        Png.AsSpan(0, offset).CopyTo(result);
        var textLength = size - Png.Length - 12;
        BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(offset, 4), textLength);
        Encoding.ASCII.GetBytes("tEXtComment\0").CopyTo(result, offset + 4);
        Array.Fill(result, (byte)'x', offset + 16, textLength - 8);
        uint crc = uint.MaxValue;
        foreach (var value in result.AsSpan(offset + 4, textLength + 4))
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) { crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xedb88320u : 0u); }
        }
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(offset + 8 + textLength, 4), ~crc);
        Png.AsSpan(Png.Length - 12).CopyTo(result.AsSpan(size - 12));
        return result;
    }

    private sealed class FailImageSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<Issue>().Any(entry => entry.Property(issue => issue.ImagePath).IsModified))
            {
                throw new DbUpdateException("Errore database simulato nel test dell'immagine.");
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
