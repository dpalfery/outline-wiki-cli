using Outlinectl.Core.Common;

namespace Outlinectl.Cli.Services;

public interface IOutputFormatter
{
    void WriteOutput<T>(T data, string commandName, MetaData? meta = null);
    void WriteError(ApiError error, string commandName, int exitCode);
    void SetFormat(OutputFormat format);
    bool IsJson { get; }
}

public enum OutputFormat
{
    Text,
    Json,
    Markdown
}
