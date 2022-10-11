using DotLiquid;
using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{

    public class GarantiConfiguration : IProviderConfiguration, I3DConfiguration
    {

        public string ProvUserId { get; set; }
        public string TerminalId { get; set; }
        public string UserId { get; set; }
        public string MerchantId { get; set; }
        public string SiteSuccessUrl { get; set; }

        [Display(Order = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string SiteFailUrl { get; set; }

        [Display(Order = 5)]
        public bool UseTestEndPoint { get; set; }

        [Display(Order = 3)]
        [DataType(DataType.Password)]
        public string SecureKey { get; set; }

        public string Type { get; set; } = "sales";
    }
}