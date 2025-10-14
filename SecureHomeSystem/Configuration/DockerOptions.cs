namespace SecureHomeSystem.Configuration;

public sealed class DockerOptions
{
    /// <summary>
    /// Relative path to the Compose file the worker references when surfacing
    /// operational commands. Defaults to the repository-managed stack definition.
    /// </summary>
    public string ComposeFilePath { get; set; } = "docker/compose.yaml";

    /// <summary>
    /// Location of the Compose environment file. Operators customise host-specific
    /// paths and credentials here during release rehearsals.
    /// </summary>
    public string EnvironmentFile { get; set; } = "docker/.env";

    /// <summary>
    /// Compose project name used to group containers. Keep this stable so screenshots
    /// and command snippets remain accurate over time.
    /// </summary>
    public string ProjectName { get; set; } = "shs-stack";

    /// <summary>
    /// Profiles automatically enabled when the worker orchestrates the stack. The
    /// default list is empty because automation is not yet wired; documentation still
    /// references the intended values.
    /// </summary>
    public string[] AutoStartProfiles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Detection settings that map Docker labels to logical service names.
    /// </summary>
    public ServiceDetectionOptions Detection { get; set; } = new();
}

public sealed class ServiceDetectionOptions
{
    /// <summary>
    /// Dictionary linking logical service identifiers to required Docker labels. Match
    /// these values with the <c>labels</c> defined in <c>docker/compose.yaml</c> when new
    /// services are added to the stack.
    /// </summary>
    public Dictionary<string, string> LabelSelector { get; set; } = new()
    {
        ["open-webui"] = "shs.role=open-webui",
        ["qdrant"] = "shs.role=qdrant",
        ["stable-diffusion"] = "shs.role=stable-diffusion"
    };

    /// <summary>
    /// Grace period (in seconds) allowed for a container to appear during startup.
    /// </summary>
    public int StartupTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Number of retries before the worker marks detection as failed. Use this during
    /// release planning to gauge how resilient the detection loop should be.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
