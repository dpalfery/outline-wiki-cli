using Outlinectl.Core.DTOs;
using Outlinectl.Core.Domain;

namespace Outlinectl.Core.Services;

public interface IOutlineApiClient
{
    Task<StandardListResponse<CollectionDto>> ListCollectionsAsync(int limit = 10, int offset = 0);
    Task<StandardListResponse<SearchResultDto>> SearchDocumentsAsync(string query, string? collectionId = null, string? parentDocumentId = null, int limit = 10, int offset = 0, bool includeArchived = false);
    Task<DocumentDto> GetDocumentAsync(string id);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentRequest request);
    Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentRequest request);
    Task<StandardListResponse<DocumentDto>> ListDocumentsAsync(string? collectionId = null, string? parentId = null, int limit = 25, int offset = 0);
}
