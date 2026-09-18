namespace TaskManagement.Api.Errors;

/// <summary>
/// Thrown when an operation would create a duplicate resource,
/// for example a user email or category name that already exists.
/// </summary>
public class DuplicateResourceException : Exception
{
    public DuplicateResourceException(string message) : base(message)
    {
    }
}
