namespace TaskManagement.Api.Validation;

/// <summary>
/// Shared "required field" checks for user-related requests, so the same rule
/// (and the same error message) is defined in one place instead of being
/// duplicated across AuthService and UserService.
/// </summary>
public static class UserValidation
{
    public static void RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required");
        }
    }

    public static void RequireEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required");
        }
    }

    public static void RequirePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required");
        }
    }

    public static void RequireNameAndEmail(string? name, string? email)
    {
        RequireName(name);
        RequireEmail(email);
    }
}
