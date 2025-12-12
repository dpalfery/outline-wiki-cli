using System.Text.Json.Serialization;

namespace Outlinectl.Core.DTOs;

public class StandardResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("pagination")]
    public Pagination? Pagination { get; set; }
}

public class StandardListResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("pagination")]
    public Pagination? Pagination { get; set; }
}

public class Pagination
{
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
    [JsonPropertyName("nextPath")]
    public string? NextPath { get; set; }
}

public class CollectionDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Url { get; set; }
    public string? Description { get; set; }
}

public class DocumentDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Text { get; set; }
    public string? StandardizedUrlId { get; set; } // URL slug?
    public string CollectionId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class SearchResponseDto
{
    // Search specific response format might differ
    [JsonPropertyName("data")]
    public List<SearchResultDto> Data { get; set; } = new();
}

public class SearchResultDto
{
    public DocumentDto Document { get; set; } = new();
    public string Context { get; set; } = ""; // Snippet
    public double Ranking { get; set; }
}

public class CreateDocumentRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = "";
    
    [JsonPropertyName("parentDocumentId")]
    public string? ParentDocumentId { get; set; }
    
    [JsonPropertyName("publish")]
    public bool Publish { get; set; } = true;
}

public class UpdateDocumentRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("append")]
    public bool Append { get; set; } = false; // Not standard API but for our internal usage maybe? No, strict mapping.
    // Outline API Update is full replacement usually unless text is optional.
    // We'll assume text replaces text.
}
