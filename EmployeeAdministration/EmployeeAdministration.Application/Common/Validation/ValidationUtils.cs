﻿namespace EmployeeAdministration.Application.Common.Validation;

public static class ValidationUtils
{
    public const string AcceptableFileExtensions = "jpg,jpeg,png,tiff";
    public const int MaxImageSize = 2_000_000;

    public const int UserNameLength = 100,
                     EmailLength = 80,
                     UserPasswordLength = 100;

    public const int ProjectNameLength = 150,
                     ProjectDescriptionLength = 400;

    public const int TaskNameLength = 150,
                     TaskDescriptionLength = 350;
}