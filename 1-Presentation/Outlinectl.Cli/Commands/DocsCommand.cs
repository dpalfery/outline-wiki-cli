using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outlinectl.Cli.Services;
using Outlinectl.Core.Services;
using System.IO;

namespace Outlinectl.Cli.Commands;

public class DocsCommand : Command
{
    public DocsCommand() : base("docs", "Manage documents")
    {
        AddCommand(CreateSearchCommand());
        AddCommand(CreateListCommand());
        AddCommand(CreateGetCommand());
        AddCommand(CreateCreateCommand());
        AddCommand(CreateUpdateCommand());
        AddCommand(CreateExportCommand());
    }

    private Command CreateSearchCommand()
    {
        var command = new Command("search", "Search for documents.");
        var queryOption = new Option<string>("--query", "Search query terms.") { IsRequired = true };
        var collectionOption = new Option<string>("--collection-id", "Filter by collection ID. Defaults to OUTLINE_COLLECTION_ID env var if set.");
        var parentOption = new Option<string>("--parent-id", "Filter by parent document ID.");
        var limitOption = new Option<int>("--limit", () => 10, "Max results.");
        var offsetOption = new Option<int>("--offset", () => 0, "Pagination offset.");
        var archivedOption = new Option<bool>("--include-archived", "Include archived documents.");

        command.AddOption(queryOption);
        command.AddOption(collectionOption);
        command.AddOption(parentOption);
        command.AddOption(limitOption);
        command.AddOption(offsetOption);
        command.AddOption(archivedOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var query = context.ParseResult.GetValueForOption(queryOption)!;
            var collectionId = context.ParseResult.GetValueForOption(collectionOption) 
                ?? Environment.GetEnvironmentVariable("OUTLINE_COLLECTION_ID");
            var parentId = context.ParseResult.GetValueForOption(parentOption);
            var limit = context.ParseResult.GetValueForOption(limitOption);
            var offset = context.ParseResult.GetValueForOption(offsetOption);
            var archived = context.ParseResult.GetValueForOption(archivedOption);

            try
            {
                var response = await docService.SearchDocumentsAsync(query, collectionId, parentId, limit, offset, archived);
                formatter.WriteOutput(response.Data, "docs.search", new Core.Common.MetaData
                {
                    Pagination = new Core.Common.PaginationMeta
                    {
                        Limit = response.Pagination?.Limit ?? limit,
                        Offset = response.Pagination?.Offset ?? offset
                    }
                });
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.search", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }

    private Command CreateListCommand()
    {
        var command = new Command("list", "List documents.");
        var collectionOption = new Option<string>("--collection-id", "Filter by collection ID. Defaults to OUTLINE_COLLECTION_ID env var if set.");
        var parentOption = new Option<string>("--parent-id", "Filter by parent document ID.");
        var limitOption = new Option<int>("--limit", () => 25, "Max results.");
        var offsetOption = new Option<int>("--offset", () => 0, "Pagination offset.");

        command.AddOption(collectionOption);
        command.AddOption(parentOption);
        command.AddOption(limitOption);
        command.AddOption(offsetOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var collectionId = context.ParseResult.GetValueForOption(collectionOption)
                ?? Environment.GetEnvironmentVariable("OUTLINE_COLLECTION_ID");
            var parentId = context.ParseResult.GetValueForOption(parentOption);
            var limit = context.ParseResult.GetValueForOption(limitOption);
            var offset = context.ParseResult.GetValueForOption(offsetOption);

            try
            {
                var response = await docService.ListDocumentsAsync(collectionId, parentId, limit, offset);
                formatter.WriteOutput(response.Data, "docs.list", new Core.Common.MetaData
                {
                    Pagination = new Core.Common.PaginationMeta
                    {
                        Limit = response.Pagination?.Limit ?? limit,
                        Offset = response.Pagination?.Offset ?? offset
                    }
                });
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.list", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }

    private Command CreateGetCommand()
    {
        var command = new Command("get", "Get a document's content.");
        var idOption = new Option<string>("--id", "Document ID.") { IsRequired = true };
        var formatOption = new Option<string>("--format", () => "markdown", "Output format (markdown|json).");

        command.AddOption(idOption);
        command.AddOption(formatOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var id = context.ParseResult.GetValueForOption(idOption)!;
            var fmt = context.ParseResult.GetValueForOption(formatOption);

            try
            {
                var doc = await docService.GetDocumentAsync(id);

                if (fmt?.ToLower() == "markdown" && !formatter.IsJson)
                {
                    formatter.SetFormat(OutputFormat.Markdown);
                    formatter.WriteOutput(doc.Text ?? string.Empty, "docs.get");
                }
                else
                {
                    // JSON or Wrapped
                    formatter.WriteOutput(doc, "docs.get");
                }
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.get", 4);
                context.ExitCode = 4;
            }
        });

        return command;
    }

    private Command CreateCreateCommand()
    {
        var command = new Command("create", "Create a new document.");
        var titleOption = new Option<string>("--title", "Document title.") { IsRequired = true };
        var collectionOption = new Option<string>("--collection-id", "Collection ID. Defaults to OUTLINE_COLLECTION_ID env var if set.");
        var textOption = new Option<string>("--text", "Document Markdown content.");
        var fileOption = new Option<string>("--file", "Path to Markdown file.");
        var stdinOption = new Option<bool>("--stdin", "Read content from stdin.");
        var parentOption = new Option<string>("--parent-id", "Parent document ID.");
        var dedupeOption = new Option<string>("--dedupe-key", "Idempotency key.");

        command.AddOption(titleOption);
        command.AddOption(collectionOption);
        command.AddOption(textOption);
        command.AddOption(fileOption);
        command.AddOption(stdinOption);
        command.AddOption(parentOption);
        command.AddOption(dedupeOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var title = context.ParseResult.GetValueForOption(titleOption)!;
            var collectionId = context.ParseResult.GetValueForOption(collectionOption) 
                ?? Environment.GetEnvironmentVariable("OUTLINE_COLLECTION_ID");
            
            if (string.IsNullOrEmpty(collectionId))
            {
                formatter.WriteError(new Core.Common.ApiError { Message = "Collection ID is required. Provide via --collection-id or OUTLINE_COLLECTION_ID env var." }, "docs.create", 1);
                context.ExitCode = 1;
                return;
            }

            var text = context.ParseResult.GetValueForOption(textOption);
            var file = context.ParseResult.GetValueForOption(fileOption);
            var useStdin = context.ParseResult.GetValueForOption(stdinOption);
            var parentId = context.ParseResult.GetValueForOption(parentOption);
            var dedupeKey = context.ParseResult.GetValueForOption(dedupeOption);

            if (useStdin)
            {
                using var reader = new StreamReader(Console.OpenStandardInput());
                text = await reader.ReadToEndAsync();
            }
            else if (!string.IsNullOrEmpty(file))
            {
                if (!File.Exists(file))
                {
                    formatter.WriteError(new Core.Common.ApiError { Message = "File not found." }, "docs.create", 2);
                    context.ExitCode = 2;
                    return;
                }
                text = await File.ReadAllTextAsync(file);
            }

            try
            {
                var doc = await docService.CreateDocumentAsync(title, collectionId, text, parentId, dedupeKey);
                formatter.WriteOutput(doc, "docs.create");
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.create", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }

    private Command CreateUpdateCommand()
    {
        var command = new Command("update", "Update an existing document.");
        var idOption = new Option<string>("--id", "Document ID.") { IsRequired = true };
        var titleOption = new Option<string>("--title", "New title.");
        var textOption = new Option<string>("--text", "New content.");
        var fileOption = new Option<string>("--file", "Path to new content file.");
        var stdinOption = new Option<bool>("--stdin", "Read content from stdin.");
        var appendOption = new Option<bool>("--append", "Append content instead of replace (if supported).");

        command.AddOption(idOption);
        command.AddOption(titleOption);
        command.AddOption(textOption);
        command.AddOption(fileOption);
        command.AddOption(stdinOption);
        command.AddOption(appendOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var id = context.ParseResult.GetValueForOption(idOption)!;
            var title = context.ParseResult.GetValueForOption(titleOption);
            var text = context.ParseResult.GetValueForOption(textOption);
            var file = context.ParseResult.GetValueForOption(fileOption);
            var useStdin = context.ParseResult.GetValueForOption(stdinOption);
            var append = context.ParseResult.GetValueForOption(appendOption);

            if (useStdin)
            {
                using var reader = new StreamReader(Console.OpenStandardInput());
                text = await reader.ReadToEndAsync();
            }
            else if (!string.IsNullOrEmpty(file))
            {
                if (!File.Exists(file))
                {
                    formatter.WriteError(new Core.Common.ApiError { Message = "File not found." }, "docs.update", 2);
                    context.ExitCode = 2;
                    return;
                }
                text = await File.ReadAllTextAsync(file);
            }

            try
            {
                var doc = await docService.UpdateDocumentAsync(id, title, text, append);
                formatter.WriteOutput(doc, "docs.update");
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.update", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }

    private Command CreateExportCommand()
    {
        var command = new Command("export", "Export a document (and optionally its children) to Markdown files.");
        var idArgument = new Argument<string>("id", "Document ID.");
        var outputOption = new Option<string>("--output-dir", () => ".", "Output directory.");
        var subtreeOption = new Option<bool>("--subtree", "Recursively export child documents.");

        command.AddArgument(idArgument);
        command.AddOption(outputOption);
        command.AddOption(subtreeOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var docService = host.Services.GetRequiredService<IDocumentService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var id = context.ParseResult.GetValueForArgument(idArgument);
            var outputDir = context.ParseResult.GetValueForOption(outputOption)!;
            var subtree = context.ParseResult.GetValueForOption(subtreeOption);

            try
            {
                await docService.ExportDocumentAsync(id, outputDir, subtree);
                formatter.WriteOutput(new { message = $"Export completed for document '{id}' to '{outputDir}'." }, "docs.export");
            }
            catch (Exception ex)
            {
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "docs.export", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }
}
