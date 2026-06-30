using BugBoard26.Api.Models;

namespace BugBoard26.Api.Services;

public static class UserRoleMapper
{
    public const string Admin = "ADMIN";
    public const string User = "USER";
    public const string Readonly = "READONLY";

    public static string ToApiRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => Admin,
            UserRole.User => User,
            UserRole.Readonly => Readonly,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    public static bool TryParse(string value, out UserRole role)
    {
        switch (value.Trim().ToUpperInvariant())
        {
            case Admin:
                role = UserRole.Admin;
                return true;
            case User:
                role = UserRole.User;
                return true;
            case Readonly:
                role = UserRole.Readonly;
                return true;
            default:
                role = default;
                return false;
        }
    }
}
