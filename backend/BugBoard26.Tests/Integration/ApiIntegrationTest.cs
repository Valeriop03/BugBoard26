using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Dtos.Auth;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

public abstract class ApiIntegrationTest : IAsyncLifetime
{
    protected const string Password = "TestPassword123!";
    protected readonly IssueApiFactory Factory = new();
    protected HttpClient Client = null!;
    protected int UserId;
    protected int IssueId;
    protected int OriginalIssueId;

    public async Task InitializeAsync()
    {
        try
        {
            await Factory.CreateDatabaseAsync();
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var users = new[]
            {
                CreateUser("ADMIN", UserRole.Admin, hasher),
                CreateUser("USER", UserRole.User, hasher),
                CreateUser("READONLY", UserRole.Readonly, hasher),
                CreateUser("INACTIVE", UserRole.User, hasher, false)
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
            UserId = users[1].Id;

            var issue = CreateIssue("Errore LOGIN", "La chiamata API restituisce un errore.");
            var original = CreateIssue("Issue originale", "Descrizione della segnalazione originale.");
            db.Issues.AddRange(issue, original);
            await db.SaveChangesAsync();
            IssueId = issue.Id;
            OriginalIssueId = original.Id;
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        try
        {
            await Factory.DisposeAsync();
        }
        finally
        {
            await Factory.DeleteDatabaseAsync();
        }
    }

    protected static string Email(string role) => $"{role.ToLowerInvariant()}@integration.bugboard26.local";

    protected async Task AuthenticateAsync(string role)
    {
        using var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = Email(role),
            password = Password
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
    }

    protected async Task<Issue> ReadIssueFromDatabaseAsync(int id)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Issues.AsNoTracking().SingleAsync(issue => issue.Id == id);
    }

    protected async Task<IssueResponse> ReadIssueFromResponseAsync(HttpResponseMessage response)
    {
        var issue = await response.Content.ReadFromJsonAsync<IssueResponse>(JsonOptions);
        Assert.NotNull(issue);
        return issue;
    }

    protected async Task<IssueResponse[]> ReadIssuesAsync(string path)
    {
        using var response = await Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var issues = await response.Content.ReadFromJsonAsync<IssueResponse[]>(JsonOptions);
        Assert.NotNull(issues);
        return issues;
    }

    protected async Task<string> SnapshotAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = await db.Users.AsNoTracking().OrderBy(user => user.Id).Select(user => new
        {
            user.Id, user.Email, user.PasswordHash, user.Role, user.IsActive, user.CreatedAt
        }).ToListAsync();
        var issues = await db.Issues.AsNoTracking().OrderBy(issue => issue.Id).Select(issue => new
        {
            issue.Id, issue.Title, issue.Description, issue.Type, issue.Priority, issue.Status,
            issue.CreatedById, issue.AssignedToId, issue.DuplicateOfIssueId, issue.ImagePath,
            issue.IsArchived, issue.CreatedAt, issue.UpdatedAt, issue.ResolvedAt
        }).ToListAsync();
        var notifications = await db.Notifications.AsNoTracking().OrderBy(notification => notification.Id).Select(notification => new
        {
            notification.Id, notification.UserId, notification.IssueId, notification.Message,
            notification.IsRead, notification.CreatedAt
        }).ToListAsync();
        return JsonSerializer.Serialize(new { users, issues, notifications });
    }

    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static User CreateUser(string role, UserRole userRole, IPasswordHasher hasher, bool isActive = true)
    {
        return new User
        {
            Email = Email(role),
            PasswordHash = hasher.Hash(Password),
            Role = userRole,
            IsActive = isActive
        };
    }

    private Issue CreateIssue(string title, string description)
    {
        return new Issue
        {
            Title = title,
            Description = description,
            Type = IssueType.Bug,
            Priority = IssuePriority.High,
            Status = IssueStatus.Todo,
            CreatedById = UserId,
            AssignedToId = UserId,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
