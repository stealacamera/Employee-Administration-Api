namespace EmployeeAdministration.Application.Common.Exceptions;

public class ExistingProjectMemberException : BaseException
{
    public ExistingProjectMemberException() : base("User is already apart of the project")
    {
    }
}
