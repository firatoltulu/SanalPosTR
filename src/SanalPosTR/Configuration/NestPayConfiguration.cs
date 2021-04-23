using DotLiquid;

namespace SanalPosTR.Configuration
{
   
    public class NestPayConfiguration : IProviderConfiguration, I3DConfiguration
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public string ClientId { get; set; }

        public string Mode { get; set; } = "P";

        public string Type { get; set; } = "Auth";

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        public bool UseTestEndPoint { get; set; }

        public virtual string Endpoint => string.Empty;

        public string HashKey { get; set; }
    }
}