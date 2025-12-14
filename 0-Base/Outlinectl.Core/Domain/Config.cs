namespace Outlinectl.Core.Domain;

public class Config
{
    public Dictionary<string, Profile> Profiles { get; set; } = new();
    public string CurrentProfile { get; set; } = "default";
}

public class Profile
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
