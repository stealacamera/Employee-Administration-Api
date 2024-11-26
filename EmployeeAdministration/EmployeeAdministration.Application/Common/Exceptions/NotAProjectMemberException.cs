namespace EmployeeAdministration.Application.Common.Exceptions;

public class NotAProjectMemberException : BaseException
{
    public NotAProjectMemberException() : base("User is not a part of the project") { }
}
