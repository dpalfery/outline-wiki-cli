using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outlinectl.Core.Services;
using Outlinectl.Cli.Services;

namespace Outlinectl.Cli.Commands;

public class CollectionsCommand : Command
{
    public CollectionsCommand() : base("collections", "Manage document collections")
    {
        AddCommand(CreateListCommand());
    }

    private Command CreateListCommand()
    {
        var command = new Command("list", "List available collections.");
        var limitOption = new Option<int>("--limit", () => 10, "Maximum number of items to return.");
        var offsetOption = new Option<int>("--offset", () => 0, "Pagination offset.");

        command.AddOption(limitOption);
        command.AddOption(offsetOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var client = host.Services.GetRequiredService<IOutlineApiClient>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var limit = context.ParseResult.GetValueForOption(limitOption);
            var offset = context.ParseResult.GetValueForOption(offsetOption);

            try
            {
                var response = await client.ListCollectionsAsync(limit, offset);
                formatter.WriteOutput(response.Data, "collections.list", new Core.Common.MetaData
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
                formatter.WriteError(new Core.Common.ApiError { Message = ex.Message }, "collections.list", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }
}
