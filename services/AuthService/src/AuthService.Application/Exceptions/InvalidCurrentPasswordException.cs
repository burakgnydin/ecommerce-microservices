namespace AuthService.Application.Exceptions;

public class InvalidCurrentPasswordException : Exception
{
    public InvalidCurrentPasswordException() : base("Current password is incorrect.")
    {
    }
}
