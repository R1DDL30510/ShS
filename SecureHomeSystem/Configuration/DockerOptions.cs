namespace SecureHomeSystem.Configuration;

public sealed class DockerOptions
{
    public string ComposeFilePath { get; set; } = "docker/compose.yaml";
    public string EnvironmentFile { get; set; } = "docker/.env";
    public string ProjectName { get; set; } = "shs-stack";
    public string[] AutoStartProfiles { get; set; } = Array.Empty<string>();
    public ServiceDetectionOptions Detection { get; set; } = new();
}

public sealed class ServiceDetectionOptions
{
    public Dictionary<string, string> LabelSelector { get; set; } = new()
    {
        ["open-webui"] = "shs.role=open-webui",
        ["qdrant"] = "shs.role=qdrant",
        ["stable-diffusion"] = "shs.role=stable-diffusion"
    };

    public int StartupTimeoutSeconds { get; set; } = 120;
    public int RetryCount { get; set; } = 3;
}
