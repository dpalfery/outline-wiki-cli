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
        
        if (profile != null && !string.IsNullOrEmpty(profile.BaseUrl))
        {
            var baseUri = new Uri(profile.BaseUrl);
            if (_httpClient.BaseAddress != baseUri)
            {
                _httpClient.BaseAddress = baseUri;
            }
        }
    }

    public async Task<StandardListResponse<CollectionDto>> ListCollectionsAsync(int limit = 10, int offset = 0)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/collections.list", new { limit, offset });
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<StandardListResponse<CollectionDto>>();
        return result ?? new StandardListResponse<CollectionDto>();
    }

    public async Task<StandardListResponse<SearchResultDto>> SearchDocumentsAsync(string query, string? collectionId = null, string? parentDocumentId = null, int limit = 10, int offset = 0)
    {
         await EnsureBaseUrlAsync();
         var payload = new 
         { 
             query, 
             collectionId, 
             parentDocumentId,
             limit, 
             offset 
         };
         
         var response = await _httpClient.PostAsJsonAsync("api/documents.search", payload);
         response.EnsureSuccessStatusCode();
         
         var result = await response.Content.ReadFromJsonAsync<StandardListResponse<SearchResultDto>>();
         return result ?? new StandardListResponse<SearchResultDto>();
    }

    public async Task<DocumentDto> GetDocumentAsync(string id)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.info", new { id });
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Document not found");
    }

    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentRequest request)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.create", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Failed to create document");
    }

    public async Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentRequest request)
    {
        await EnsureBaseUrlAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents.update", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<StandardResponse<DocumentDto>>();
        return result?.Data ?? throw new Exception("Failed to update document");
    }

    public async Task<StandardListResponse<DocumentDto>> ListDocumentsAsync(string? collectionId = null, string? parentId = null, int limit = 25, int offset = 0)
    {
        await EnsureBaseUrlAsync();
        // Uses documents.list
        var payload = new { collectionId, parentDocumentId = parentId, limit, offset };
        var response = await _httpClient.PostAsJsonAsync("api/documents.list", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<StandardListResponse<DocumentDto>>();
        return result ?? new StandardListResponse<DocumentDto>();
    }
}
