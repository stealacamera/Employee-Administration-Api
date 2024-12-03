namespace EmployeeAdministration.Infrastructure.Options;

internal class MessageBrokerOptions
{
    public static string SectionName = "MessageBroker";

    public string HostName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
