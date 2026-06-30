using BugBoard26.Api.Data;
using BugBoard26.Api.Dtos.Users;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoleMapper.Admin)]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers()
    {
        var users = await _dbContext.Users
            .OrderBy(user => user.Email)
            .Select(user => ToResponse(user))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        if (!UserRoleMapper.TryParse(request.Role, out var role))
        {
            return BadRequest(new { message = "Ruolo non valido. Valori ammessi: ADMIN, USER, READONLY." });
        }

        var email = NormalizeEmail(request.Email);
        var alreadyExists = await _dbContext.Users.AnyAsync(user => user.Email == email);

        if (alreadyExists)
        {
            return Conflict(new { message = "Esiste gia' un utente con questa email." });
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Created("/api/users", ToResponse(user));
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = UserRoleMapper.ToApiRole(user.Role),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
