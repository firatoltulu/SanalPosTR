using SimplePayTR.Core.Configuration;

namespace SimplePayTR.Core.Configuration
{
    public class YKBConfiguration : IProviderConfiguration, I3DConfiguration
    {
        /// <summary>
        /// Üye iş yeri no
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Terminal no
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// Posnet no
        /// </summary>
        public string PosnetId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        public bool UseTestEndPoint { get; set; }

        public virtual string Endpoint => string.Empty;

        public string HashKey { get; set; }
    }
}