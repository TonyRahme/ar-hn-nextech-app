using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hn.Api.Models
{
    public record ItemDto
    (
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("deleted")] bool Deleted,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("by")] string? By,
        [property: JsonPropertyName("time")] long Time,
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("dead")] bool Dead,
        [property: JsonPropertyName("parent")] int? Parent,
        [property: JsonPropertyName("poll")] int? Poll,
        [property: JsonPropertyName("kids")] int[]? Kids,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("score")] int? Score,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("parts")] int[]? Parts,
        [property: JsonPropertyName("descendants")] int? Descendants
    );
}