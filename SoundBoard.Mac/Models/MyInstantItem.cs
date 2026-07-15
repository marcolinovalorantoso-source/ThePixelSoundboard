using System.Text.Json.Serialization;

namespace SoundBoard.Models
{
    public record MyInstantItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("sound")] string SoundUrl,
        [property: JsonPropertyName("color")] string? Color,
        [property: JsonPropertyName("description")] string? Description
    );

    public record MyInstantsResponse(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("next")] string? Next,
        [property: JsonPropertyName("previous")] string? Previous,
        [property: JsonPropertyName("results")] List<MyInstantItem> Results
    );
}
