using BugBoard26.Api.Data;
using BugBoard26.Api.Dtos.Auth;
using BugBoard26.Api.Dtos.Users;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Email o password non valide." });
        }

        var token = _jwtTokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = ToResponse(user)
        });
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
