using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outlinectl.Cli.Services;
using Outlinectl.Core.Common;
using Outlinectl.Core.Services;

namespace Outlinectl.Cli.Commands;

public class AuthCommand : Command
{
    public AuthCommand() : base("auth", "Manage authentication and profiles")
    {
        AddCommand(CreateLoginCommand());
        AddCommand(CreateLogoutCommand());
        AddCommand(CreateStatusCommand());
    }

    private Command CreateLoginCommand()
    {
        var command = new Command("login", "Authenticate with an Outline instance.");
        var baseUrlOption = new Option<string>("--base-url", "The URL of your Outline instance (e.g. https://docs.example.com).") { IsRequired = true };
        var tokenOption = new Option<string>("--token", "The API token (can also be passed via stdin or OUTLINE_API_TOKEN).");
        var tokenStdinOption = new Option<bool>("--token-stdin", "Read token from stdin.");
        var profileOption = new Option<string>("--profile", () => "default", "Configuration profile name.");

        command.AddOption(baseUrlOption);
        command.AddOption(tokenOption);
        command.AddOption(tokenStdinOption);
        command.AddOption(profileOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var authService = host.Services.GetRequiredService<IAuthService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var baseUrl = context.ParseResult.GetValueForOption(baseUrlOption)!;
            var token = context.ParseResult.GetValueForOption(tokenOption);
            var useStdin = context.ParseResult.GetValueForOption(tokenStdinOption);
            var profile = context.ParseResult.GetValueForOption(profileOption)!;

            if (useStdin)
            {
                // Read from stdin
                using var reader = new StreamReader(Console.OpenStandardInput());
                token = await reader.ReadToEndAsync();
                token = token.Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                formatter.WriteError(new ApiError { Message = "Token is required via --token, --token-stdin." }, "auth.login", 2);
                context.ExitCode = 2;
                return;
            }

            try
            {
                await authService.LoginAsync(baseUrl, token, profile);
                formatter.WriteOutput(new { message = $"Successfully logged in to profile '{profile}'." }, "auth.login");
                context.ExitCode = 0;
            }
            catch (Exception ex)
            {
                formatter.WriteError(new ApiError { Message = ex.Message }, "auth.login", 10);
                context.ExitCode = 10;
            }
        });

        return command;
    }

    private Command CreateLogoutCommand()
    {
        var command = new Command("logout", "Remove authentication credentials.");
        var profileOption = new Option<string>("--profile", () => "default", "Configuration profile name.");
        command.AddOption(profileOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var authService = host.Services.GetRequiredService<IAuthService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();
            var profile = context.ParseResult.GetValueForOption(profileOption)!;

            await authService.LogoutAsync(profile);
            formatter.WriteOutput(new { message = $"Logged out from profile '{profile}'." }, "auth.logout");
        });

        return command;
    }

    private Command CreateStatusCommand()
    {
        var command = new Command("status", "Verify authentication status.");
        
        command.SetHandler(async (InvocationContext context) =>
        {
            var host = context.GetHost();
            var authService = host.Services.GetRequiredService<IAuthService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();

            var profileName = await authService.GetCurrentProfileNameAsync();
            var profile = await authService.GetProfileAsync(profileName);
            var token = await authService.GetTokenAsync(profileName);

            if (profile == null)
            {
                formatter.WriteError(new ApiError { Message = "Not logged in. No profile found." }, "auth.status", 3);
                context.ExitCode = 3;
                return;
            }

            // TODO: Ping API using IOutlineApiClient (Task 3)
            // For now, checks local config.
            bool hasToken = !string.IsNullOrEmpty(token);

            var status = new
            {
                profile = profileName,
                baseUrl = profile.BaseUrl,
                authenticated = hasToken,
                status = hasToken ? "OK (Local)" : "Missing Token" 
            };
            
            formatter.WriteOutput(status, "auth.status");
        });

        return command;
    }
}
