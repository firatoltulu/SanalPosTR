using SimplePayTR.Core.Configuration;

namespace SimplePayTR.Core.Providers.Est
{
    public class EstConfiguration : IProviderConfiguration, I3DConfiguration
    {
        public string Endpoint { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string ClientId { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }
    }
}