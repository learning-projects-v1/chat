namespace ChatApp.Application.Exceptions;

/// <summary>
/// Thrown by IUnitOfWork.SaveChangesAsync when a unique key / primary key is violated.
/// Lets Application code react to "this row already exists" without knowing about
/// EF Core or PostgreSQL error codes (Infrastructure translates them into this).
/// </summary>
public sealed class DuplicateKeyException : Exception
{
    public DuplicateKeyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
