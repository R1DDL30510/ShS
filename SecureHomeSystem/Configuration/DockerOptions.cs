namespace SecureHomeSystem.Configuration
{
    public sealed class DockerOptions
    {
        public string ComposeFile { get; set; } = "docker/compose.yaml";

        public string ProjectName { get; set; } = "shs-stack";

        public string[] Profiles { get; set; } = ["worker"];
    }
}
