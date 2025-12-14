using System.Net.Http.Json;
using System.Text.Json;
using Outlinectl.Core.DTOs;
using Outlinectl.Core.Services;

namespace Outlinectl.Api;

public class OutlineApiClient : IOutlineApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public OutlineApiClient(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task EnsureBaseUrlAsync()
    {
        // We set the BaseAddress here dynamically because it depends on the profile.
        // Alternatively, AuthHeaderHandler could set it? No, Handler handles request modification.
        // Ideally we configure the HttpClient with the base address in DI, but base address changes.
        // So we set it on the request or check here.
        
        var profileName = await _authService.GetCurrentProfileNameAsync();
        var profile = await _authService.GetProfileAsync(profileName);
        
        if (profile == null || string.IsNullOrWhiteSpace(profile.BaseUrl))
        {
            throw new InvalidOperationException("No Outline base URL configured. Run `outlinectl auth login` to set a profile.");
        }

        var baseUri = new Uri(profile.BaseUrl);
        if (_httpClient.BaseAddress != baseUri)
        {
            _httpClient.BaseAddress = baseUri;
        }
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = string.Empty;
        try
        {
            body = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            // Ignore body read errors; we'll still throw a useful exception.
        }

        var request = response.RequestMessage;
        var requestSummary = request != null
            ? $"{request.Method} {request.RequestUri}"
            : "<unknown request>";

        var status = (int)response.StatusCode;
        var reason = response.ReasonPhrase;

        string extractedMessage = TryExtractMessageFromJson(body);
        var bodySnippet = Truncate(extractedMessage.Length > 0 ? extractedMessage : body, 4096);

        var message = $"Outline API request failed: {requestSummary} -> {status} {reason}.";
        if (!string.IsNullOrWhiteSpace(bodySnippet))
        {
            message += $" Body: {bodySnippet}";
        }

        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, maxLength) + "…";
    }

    private static string TryExtractMessageFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Common Outline-ish / API patterns
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                {
                    return messageProp.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("error", out var errorProp))
                {
                    if (errorProp.ValueKind == JsonValueKind.String)
                    {
                        return errorProp.GetString() ?? string.Empty;
                    }

                    if (errorProp.ValueKind == JsonValueKind.Object &&
                        errorProp.TryGetProperty("message", out var nestedMessage) &&
                        nestedMessage.ValueKind == JsonValueKind.String)
                    {
                        return nestedMessage.GetString() ?? string.Empty;
                    }
                }
            }
        }
        catch
        {
            // Not JSON or unparsable; fall back to raw body.
        }

        return string.Empty;
    }

    public async Task<StandardListResponse<CollectionDto>> ListCollectionsAsync(int limit = 10, int offset = 0)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/collections.list", new { limit, offset });
        await EnsureSuccessOrThrowAsync(response);
        
        var result = await response.Content.ReadFromJsonAsync<StandardListResponse<CollectionDto>>();
        return result ?? new StandardListResponse<CollectionDto>();
    }

    public async Task<StandardListResponse<SearchResultDto>> SearchDocumentsAsync(string query, string? collectionId = null, string? parentDocumentId = null, int limit = 10, int offset = 0, bool includeArchived = false)
    {
         await EnsureBaseUrlAsync();
         var payload = new 
         { 
             query, 
             collectionId, 
             parentDocumentId,
             limit, 
             offset,
             includeArchived 
         };
         
         var response = await _httpClient.PostAsJsonAsync("api/documents.search", payload);
            await EnsureSuccessOrThrowAsync(response);
         
         var result = await response.Content.ReadFromJsonAsync<StandardListResponse<SearchResultDto>>();
         return result ?? new StandardListResponse<SearchResultDto>();
    }

    public async Task<DocumentDto> GetDocumentAsync(string id)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.info", new { id });
        await EnsureSuccessOrThrowAsync(response);
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Document not found");
    }

    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentRequest request)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.create", request);
        await EnsureSuccessOrThrowAsync(response);
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Failed to create document");
    }

    public async Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentRequest request)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.update", request);
        await EnsureSuccessOrThrowAsync(response);
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Failed to update document");
    }

    public async Task<StandardListResponse<DocumentDto>> ListDocumentsAsync(string? collectionId = null, string? parentId = null, int limit = 25, int offset = 0)
    {
        await EnsureBaseUrlAsync();
        // Uses documents.list
        var payload = new { collectionId, parentDocumentId = parentId, limit, offset };
        var response = await _httpClient.PostAsJsonAsync("api/documents.list", payload);
        await EnsureSuccessOrThrowAsync(response);

        var result = await response.Content.ReadFromJsonAsync<StandardListResponse<DocumentDto>>();
        return result ?? new StandardListResponse<DocumentDto>();
    }
}
