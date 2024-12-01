using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmployeeAdministration.Domain.Enums;

namespace EmployeeAdministration.Application.Common;

public sealed class ProjectStatusesJsonConverter : JsonConverter<ProjectStatuses>
{
    public override ProjectStatuses? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ProjectStatuses status;

        if (!ProjectStatuses.TryFromName(reader.GetString(), ignoreCase: true, out status))
            throw new InvalidEnumArgumentException(nameof(ProjectStatuses));

        return status;
    }

    public override void Write(Utf8JsonWriter writer, ProjectStatuses value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Name);
}
