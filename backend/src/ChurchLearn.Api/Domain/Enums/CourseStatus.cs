using System.Text.Json.Serialization;

namespace ChurchLearn.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseStatus
{
    Draft,
    Published,
    Archived
}
