using Outlinectl.Cli.Services;
using Outlinectl.Core.Common;

namespace Outlinectl.Tests;

public class OutputFormatterTests
{
    [Fact]
    public void WriteOutput_ShouldRespectQuietFlag()
    {
        var formatter = new OutputFormatter();
        formatter.SetQuiet(true);

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            formatter.WriteOutput(new { message = "hidden" }, "test.command");
            formatter.WriteError(new ApiError { Message = "visible" }, "test.command", 1);

            Assert.Equal(string.Empty, output.ToString());
            Assert.Contains("visible", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
