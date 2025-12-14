using System.Text.Json.Serialization;

namespace Outlinectl.Core.Domain;

public record Document
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Text { get; init; } // Markdown content
    public required string Url { get; init; }
    public required string CollectionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
