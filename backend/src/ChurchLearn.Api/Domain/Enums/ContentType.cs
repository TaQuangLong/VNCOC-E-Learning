using System.Text.Json.Serialization;

namespace ChurchLearn.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    Video,
    Text,
    Pdf
}
