using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outlinectl.Api;
using Outlinectl.Cli.Services;
using Outlinectl.Core.Common;
using Outlinectl.Core.Services;
using Outlinectl.Storage;
using Serilog;
using Serilog.Events;

namespace Outlinectl.Cli;

class Program
{
    private static IReadOnlyList<string> SplitCommandLine(string input)
    {
        var args = new List<string>();
        if (string.IsNullOrWhiteSpace(input)) return args;

        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == quoteChar)
                {
                    inQuotes = false;
                    continue;
                }

                if (c == '\\' && i + 1 < input.Length)
                {
                    // Basic escaping inside quotes: \" or \\ 
                    char next = input[i + 1];
                    if (next == quoteChar || next == '\\')
                    {
                        current.Append(next);
                        i++;
                        continue;
                    }
                }

                current.Append(c);
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            if (c == '"' || c == '\'')
            {
                inQuotes = true;
                quoteChar = c;
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args;
    }

    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Outlinectl: CLI for Outline Wiki");

        // Global options
        var jsonOption = new Option<bool>("--json", "Output strictly valid JSON.");
        var quietOption = new Option<bool>("--quiet", "Suppress all non-error output.");
        var verboseOption = new Option<bool>("--verbose", "Enable verbose logging.");

        rootCommand.AddGlobalOption(jsonOption);
        rootCommand.AddGlobalOption(quietOption);
        rootCommand.AddGlobalOption(verboseOption);

        // We need to resolve the command later from DI or instantiate it here if it has dependencies.
        // System.CommandLine.Hosting handles instantiation if we use `.UseCommand<AuthCommand>()` but typical UseHost pattern is slightly different.
        // Actually, we should add the command instance to rootCommand. 
        // But since we are inside `Main` and don't have the host yet, we can't resolve it.
        // A common pattern with System.CommandLine.Hosting is to add the Type and let the Host resolve it, OR
        // just instantiate it if dependencies are minimal. But AuthCommand has dependencies.
        
        // Better approach: Use `result.Host.Services.GetRequiredService<AuthCommand>()` is tricky because we build the parser BEFORE the host.
        // Wait, `UseHost` builds the host.
        // But `rootCommand` is defined before.
        
        // Solution: Defines subcommands as simple instances that DELAY resolution or use the handler to resolve.
        // My AuthCommand constructor adds subcommands. The Handlers inside resolve from `context.GetHost()`.
        // So AuthCommand ITSELF does not need dependencies in Constructor!
        // Let's check AuthCommand constructor: `public AuthCommand() ...`. It takes NO arguments.
        // Dependencies are resolved inside `SetHandler`.
        // So I can just `new AuthCommand()` here!
        
        rootCommand.AddCommand(new Outlinectl.Cli.Commands.AuthCommand());
        rootCommand.AddCommand(new Outlinectl.Cli.Commands.CollectionsCommand());
        rootCommand.AddCommand(new Outlinectl.Cli.Commands.DocsCommand());

        Parser? parser = null;
        var shellCommand = new Command("shell", "Run outlinectl in interactive mode (type 'exit' to quit).")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        shellCommand.SetHandler(async (InvocationContext context) =>
        {
            Console.WriteLine("outlinectl shell — type 'help' for usage, 'exit' to quit");
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null) break;

                line = line.Trim();
                if (line.Length == 0) continue;

                if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var commandArgs = SplitCommandLine(line);
                if (commandArgs.Count == 0) continue;

                if (string.Equals(commandArgs[0], "help", StringComparison.OrdinalIgnoreCase))
                {
                    await (parser ?? throw new InvalidOperationException("Parser not initialized.")).InvokeAsync(new[] { "--help" });
                    continue;
                }

                await (parser ?? throw new InvalidOperationException("Parser not initialized.")).InvokeAsync(commandArgs.ToArray());
            }

            context.ExitCode = 0;
        });

        rootCommand.AddCommand(shellCommand);

        parser = new CommandLineBuilder(rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(), host =>
            {
                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IOutputFormatter, OutputFormatter>();
                    
                    // Core Services
                    services.AddSingleton<IStore, FileStore>();
                    services.AddSingleton<ISecureStore, KeyStore>();
                    services.AddSingleton<IAuthService, AuthService>();

                    // Commands
                    services.AddSingleton<Outlinectl.Cli.Commands.AuthCommand>();
                    services.AddSingleton<Outlinectl.Cli.Commands.CollectionsCommand>();
                    services.AddSingleton<Outlinectl.Cli.Commands.DocsCommand>();

                    // API
                    services.AddTransient<Outlinectl.Api.AuthHeaderHandler>();
                    services.AddHttpClient<IOutlineApiClient, OutlineApiClient>()
                        .AddHttpMessageHandler<Outlinectl.Api.AuthHeaderHandler>()
                        .AddStandardResilienceHandler(); // Polly defaults
                    
                    services.AddSingleton<IDocumentService, DocumentService>();

                });

                host.UseSerilog((context, services, configuration) =>
                {
                    var parseResult = services.GetService<ParseResult>();
                    bool isJson = parseResult?.GetValueForOption(jsonOption) ?? false;
                    bool isQuiet = parseResult?.GetValueForOption(quietOption) ?? false;
                    bool isVerbose = parseResult?.GetValueForOption(verboseOption) ?? false;

                    if (isJson || isQuiet)
                    {
                        // In JSON or Quiet mode, standard logs shouldn't pollute stdout.
                        // We might log to file or stderr depending on needs.
                        // For now, suppress console logging unless verbose is on (to stderr).
                        configuration.WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose, outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}");
                        if (!isVerbose) configuration.MinimumLevel.Warning();
                    }
                    else
                    {
                        configuration.WriteTo.Console();
                        configuration.MinimumLevel.Information();
                    }
                });
            })
            .UseDefaults()
            .AddMiddleware(async (context, next) =>
            {
                var host = context.GetHost();
                var formatter = host.Services.GetRequiredService<IOutputFormatter>();

                var parseResult = context.ParseResult;
                var isJson = parseResult.GetValueForOption(jsonOption);
                var isQuiet = parseResult.GetValueForOption(quietOption);

                formatter.SetFormat(isJson ? OutputFormat.Json : OutputFormat.Text);
                formatter.SetQuiet(isQuiet);

                await next(context);
            })
            .UseExceptionHandler((ex, context) =>
            {
                var host = context.GetHost();
                var formatter = host.Services.GetRequiredService<IOutputFormatter>();
                var parseResult = context.ParseResult;
                bool isJson = parseResult.GetValueForOption(jsonOption);

                if (isJson) formatter.SetFormat(OutputFormat.Json);

                var exitCode = 10; // Unknown
                var error = new ApiError { Message = ex.Message };

                if (ex is OperationCanceledException)
                {
                    exitCode = 130;
                    error.Code = "CANCELLED";
                }
                // Map other exceptions here
                
                formatter.WriteError(error, parseResult.CommandResult.Command.Name, exitCode);
                context.ExitCode = exitCode;
            })
            .Build();

        return await parser.InvokeAsync(args);
    }
}
