namespace Outlinectl.Core.Domain;

public record Collection
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
}
