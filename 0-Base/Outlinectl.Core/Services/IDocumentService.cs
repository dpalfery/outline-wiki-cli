using Outlinectl.Core.DTOs;

namespace Outlinectl.Core.Services;

public interface IDocumentService
{
    Task<StandardListResponse<SearchResultDto>> SearchDocumentsAsync(string query, string? collectionId, string? parentDocumentId, int limit, int offset, bool includeArchived);
    Task<StandardListResponse<DocumentDto>> ListDocumentsAsync(string? collectionId, string? parentId, int limit, int offset);
    Task<DocumentDto> GetDocumentAsync(string id);
    Task<DocumentDto> CreateDocumentAsync(string title, string collectionId, string? text, string? parentId, string? dedupeKey);
    Task<DocumentDto> UpdateDocumentAsync(string id, string? title, string? text, bool append);
    Task ExportDocumentAsync(string id, string outputDir, bool subtree);
}
