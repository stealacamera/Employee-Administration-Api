namespace EmployeeAdministration.Application.Common.Exceptions;

public class UncompletedTasksAssignedToEntityException : BaseException
{
    public UncompletedTasksAssignedToEntityException(string entityName) : 
        base($"Cannot perform operation because there are uncompleted tasks for this {entityName.ToLower()}")
    {        
    }
}
