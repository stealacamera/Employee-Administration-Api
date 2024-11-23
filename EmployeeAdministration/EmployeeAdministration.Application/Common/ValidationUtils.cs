namespace EmployeeAdministration.Application.Common;

public static class ValidationUtils
{
    public const string AcceptableFileExtensions = "jpg,jpeg,png,tiff";

    public const int UserNameLength = 100,
                     EmailLength = 80,
                     UserPasswordLength = 100;
}
