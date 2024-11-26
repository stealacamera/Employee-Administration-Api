namespace EmployeeAdministration.Infrastructure.Options;

internal class CloudinaryOptions
{

    public static string SectionName = "Cloudinary";

    public string Name { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret {  get; set; } = null!;
}
