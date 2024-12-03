namespace EmployeeAdministration.Infrastructure.Options;

internal class EmailOptions
{
    public static string SectionName = "Email";

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string Host { get; set; } = null!;
    public int Port { get; set; }
}
