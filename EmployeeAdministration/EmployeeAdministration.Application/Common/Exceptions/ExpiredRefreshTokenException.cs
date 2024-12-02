namespace EmployeeAdministration.Application.Common.Exceptions;

public class ExpiredRefreshTokenException : BaseException
{
    public ExpiredRefreshTokenException() : base("Refresh token is expired")
    {
    }
}
