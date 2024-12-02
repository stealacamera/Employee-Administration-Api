namespace EmployeeAdministration.Application.Common.Exceptions.General;

public class EntityNotFoundException : BaseException
{
    public EntityNotFoundException(string entityName) : base($"{entityName} could not be found")
    {
    }
}
