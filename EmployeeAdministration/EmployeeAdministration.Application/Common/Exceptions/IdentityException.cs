using Microsoft.AspNetCore.Identity;

namespace EmployeeAdministration.Application.Common.Exceptions;

public class IdentityException : Exception
{
    public Dictionary<string, string[]> Errors { get; private set; }

    public IdentityException(IdentityResult result)
        => Errors = GroupIdentityErrors(result.Errors);

    private Dictionary<string, string[]> GroupIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var groupedErrors = new Dictionary<string, string[]>();

        foreach (var error in errors)
        {
            string errorTitle;

            if (error.Code.Contains("Password"))
                errorTitle = "Password";
            else if (error.Code.Contains("Role"))
                errorTitle = "Role";
            else if (error.Code.Contains("UserName"))
                errorTitle = "Username";
            else if (error.Code.Contains("Email"))
                errorTitle = "Email";
            else
                errorTitle = "Other";

            if (groupedErrors.ContainsKey(errorTitle))
                groupedErrors[errorTitle].Append(error.Description);
            else
                groupedErrors.Add(errorTitle, [error.Description]);
        }

        return groupedErrors;
    }
}
