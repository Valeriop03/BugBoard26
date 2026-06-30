using BugBoard26.Api.Models;

namespace BugBoard26.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
