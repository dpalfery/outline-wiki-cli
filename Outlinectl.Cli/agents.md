# Agent Guidelines for Outlinectl.Cli

## Project Purpose

Outlinectl.Cli is the main executable that provides the command-line user interface. It orchestrates all other projects and provides the user-facing experience.

## Architecture Role

- **Application Entry Point**: Main executable project
- **Command Orchestration**: Defines and implements CLI commands
- **User Experience**: Handles output formatting and error messages
- **Dependency Composition**: Wires up all services via DI

## Key Principles

### User-Centric Design
- Clear, helpful error messages
- Consistent command structure
- Intuitive option names
- Comprehensive help text

### Output Flexibility
- Support human-readable output
- Support JSON for scripting
- Support quiet mode for automation
- Respect user preferences

### Separation of Concerns
- Commands handle CLI concerns only
- Business logic lives in services (Core)
- API calls go through IOutlineApiClient
- No direct HTTP in commands

## Coding Guidelines

### Command Structure
```csharp
public class MyCommand : Command
{
    public MyCommand() : base("commandname", "Description of command")
    {
        // Define options
        var option = new Option<string>(
            "--option",
            "Option description"
        ) { IsRequired = true };
        
        AddOption(option);
        
        // Define handler
        this.SetHandler(async (InvocationContext context) =>
        {
            // Get services from DI
            var host = context.GetHost();
            var service = host.Services.GetRequiredService<IMyService>();
            var formatter = host.Services.GetRequiredService<IOutputFormatter>();
            
            // Get option values
            var optionValue = context.ParseResult.GetValueForOption(option);
            
            // Execute business logic
            var result = await service.DoWorkAsync(optionValue);
            
            // Format output
            formatter.WriteResult(result);
            
            context.ExitCode = 0;
        });
    }
}
```

### Output Formatting
```csharp
// Human-readable output
formatter.WriteLine("Collection: {0}", collection.Name);

// JSON output (respects --json flag)
formatter.WriteJson(collection);

// Error output
formatter.WriteError(new ApiError 
{ 
    Message = "Not found",
    Code = "NOT_FOUND" 
}, "command", exitCode: 404);
```

### Error Handling
```csharp
try
{
    await service.DoWorkAsync();
}
catch (HttpRequestException ex)
{
    formatter.WriteError(
        new ApiError { Message = ex.Message },
        context.ParseResult.CommandResult.Command.Name,
        exitCode: 1
    );
    context.ExitCode = 1;
}
```

## Common Modifications

### Adding a New Command
1. Create `Commands/MyCommand.cs`
2. Inherit from `Command`
3. Define constructor with options
4. Implement handler
5. Register in `Program.cs`:
   ```csharp
   rootCommand.AddCommand(new MyCommand());
   services.AddSingleton<MyCommand>();
   ```
6. Add tests
7. Update README

### Adding a Global Option
1. Define option in `Program.cs`
2. Add to `rootCommand.AddGlobalOption()`
3. Access in handlers via `ParseResult`
4. Update documentation

### Adding a Subcommand
```csharp
public class ParentCommand : Command
{
    public ParentCommand() : base("parent", "Description")
    {
        AddCommand(new ChildCommand());
    }
}
```

### Modifying Output Format
1. Update `OutputFormatter` class
2. Respect output mode (Human/Json)
3. Test both modes
4. Update examples

## Testing Considerations

### Integration Tests
```csharp
[Fact]
public async Task AuthLogin_Success()
{
    // Arrange
    var args = new[] { "auth", "login", "--token", "test" };
    
    // Act
    var exitCode = await Program.Main(args);
    
    // Assert
    Assert.Equal(0, exitCode);
}
```

### Unit Tests
- Mock services (IAuthService, IOutlineApiClient)
- Test option parsing
- Verify output formatting
- Check exit codes

### Output Validation
- Capture stdout/stderr
- Parse JSON output
- Verify human-readable format
- Test error messages

## Common Pitfalls

- ❌ Business logic in command handlers
- ❌ Direct HTTP calls from commands
- ❌ Hardcoded output format (ignoring --json)
- ❌ Inconsistent error handling
- ✅ Delegate to service layer
- ✅ Use IOutlineApiClient for API calls
- ✅ Respect output mode flags
- ✅ Centralized error handling

## User Experience Guidelines

### Command Naming
- Use verbs for actions: `create`, `delete`, `list`
- Use nouns for resources: `docs`, `collections`, `auth`
- Keep names short and intuitive
- Follow industry conventions

### Help Text
- Provide clear descriptions
- Include usage examples
- Document all options
- Explain exit codes

### Error Messages
- Be specific and actionable
- Suggest solutions when possible
- Include error codes for scripting
- Respect output mode (--json)

### Progress Indication
- Show progress for long operations
- Support cancellation (Ctrl+C)
- Provide feedback for user actions
- Use appropriate log levels

## Interactive Shell

### Command Parsing
- Handle quoted arguments: `"string with spaces"`
- Support escaping: `\"` and `\\`
- Split on whitespace
- Preserve quotes in values

### Shell Commands
- `help`: Show help text
- `exit` or `quit`: Exit shell
- Regular commands work as normal
- Support command history

### Implementation
```csharp
var commandArgs = SplitCommandLine(line);
await parser.InvokeAsync(commandArgs.ToArray());
```

## Logging Best Practices

### Log Levels
- **Verbose**: Detailed diagnostic info
- **Information**: General informational messages
- **Warning**: Unexpected but recoverable
- **Error**: Errors that should be addressed

### Logging Context
```csharp
Log.Information("Fetching document {DocumentId}", documentId);
Log.Error(ex, "Failed to authenticate profile {Profile}", profileName);
```

### Output vs Logging
- Output (Console.WriteLine): User-facing results
- Logging (Serilog): Diagnostic information
- Respect --quiet flag
- JSON mode: minimize logs

## Dependency Injection

### Service Registration
```csharp
// Singleton for stateless services
services.AddSingleton<IStore, FileStore>();

// Transient for stateful or per-request
services.AddTransient<AuthHeaderHandler>();

// HttpClient with handlers
services.AddHttpClient<IOutlineApiClient, OutlineApiClient>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddStandardResilienceHandler();
```

### Service Resolution
```csharp
var host = context.GetHost();
var service = host.Services.GetRequiredService<IMyService>();
```

## Security Considerations

- Never log tokens or credentials
- Sanitize user input
- Validate file paths
- Handle Ctrl+C gracefully
- Clear sensitive data from memory
