using System.Text.Json;
using Outlinectl.Core.Common;

namespace Outlinectl.Cli.Services;

public class OutputFormatter : IOutputFormatter
{
    private OutputFormat _format = OutputFormat.Text;
    private bool _quiet;
    private readonly JsonSerializerOptions _jsonOptions;

    public OutputFormatter()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public void SetFormat(OutputFormat format)
    {
        _format = format;
    }

    public void SetQuiet(bool quiet)
    {
        _quiet = quiet;
    }

    public bool IsJson => _format == OutputFormat.Json;

    public void WriteOutput<T>(T data, string commandName, MetaData? meta = null)
    {
        if (_quiet)
        {
            return;
        }

        if (_format == OutputFormat.Json)
        {
            var envelope = new JsonEnvelope<T>
            {
                Ok = true,
                Command = commandName,
                Data = data,
                Meta = meta ?? new MetaData { DurationMs = 0 } // Duration injected by middleware potentially
            };
            Console.WriteLine(JsonSerializer.Serialize(envelope, _jsonOptions));
        }
        else
        {
            // Human readable output
            // For MVP, just ToString or simple rendering. 
            // Better to have specialized renderers, but for now generic.
            if (_format == OutputFormat.Markdown && data is string s)
            {
                Console.WriteLine(s);
            }
            else if (data is string str)
            {
                Console.WriteLine(str);
            }
            else
            {
                // Fallback to simpler JSON or ToString for objects in text mode
                Console.WriteLine(JsonSerializer.Serialize(data, _jsonOptions));
            }
        }
    }

    public void WriteError(ApiError error, string commandName, int exitCode)
    {
        if (_format == OutputFormat.Json)
        {
            var envelope = new JsonEnvelope<object>
            {
                Ok = false,
                Command = commandName,
                Error = error,
                Meta = new MetaData { DurationMs = 0 }
            };
            Console.WriteLine(JsonSerializer.Serialize(envelope, _jsonOptions));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error: {error.Message}");
            if (!string.IsNullOrEmpty(error.Hint))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine($"Hint: {error.Hint}");
            }
            Console.ResetColor();
        }
    }
}
