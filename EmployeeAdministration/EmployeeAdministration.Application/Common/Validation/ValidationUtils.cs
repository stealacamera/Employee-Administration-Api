namespace EmployeeAdministration.Application.Common.Validation;

public static class ValidationUtils
{
    public const string AcceptableFileExtensions = ".jpg,.jpeg,.png";
    public const int MaxImageBytes = 2_000_000;

    public const int UserNameLength = 100,
                     UserEmailLength = 80,
                     UserPasswordLength = 100;

    public const int ProjectNameLength = 150,
                     ProjectDescriptionLength = 400;

    public const int TaskNameLength = 150,
                     TaskDescriptionLength = 350;

    public const string PasswordRegex = @"^(?=.*\d)(?=.*[a-zA-Z]).{8,}$",
                        PasswordRegexErrorMessage = "Passwords require a minimum of 8 characters (letters and numbers)";

    public const int MaxEmployeesPerTransaction = 100;
}
