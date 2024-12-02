namespace EmployeeAdministration.Infrastructure.Common;

internal static class CacheKeys
{
    public static string UserRole(int userId) => $"user-role-{userId}";
}
