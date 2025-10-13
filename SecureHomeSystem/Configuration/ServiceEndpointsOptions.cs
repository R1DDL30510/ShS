namespace SecureHomeSystem.Configuration
{
    public sealed class ServiceEndpointsOptions
    {
        public ServiceEndpoint Ollama { get; set; } = new()
        {
            Url = "http://localhost:11434"
        };

        public ServiceEndpoint OpenWebUI { get; set; } = new()
        {
            Url = "http://localhost:3000"
        };

        public ServiceEndpoint StableDiffusion { get; set; } = new()
        {
            Url = "http://localhost:7860"
        };

        public sealed class ServiceEndpoint
        {
            public string Url { get; set; } = string.Empty;
        }
    }
}
