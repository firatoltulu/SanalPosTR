using DotLiquid;
using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{

    public class NestPayConfiguration : IProviderConfiguration, I3DConfiguration
    {
        [Display(Order = 0)]
        public string Name { get; set; }

        [Display(Order = 1)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Order = 2)]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        public string Mode { get; set; } = "P";

        public string Type { get; set; } = "Auth";

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        [Display(Order = 4)]
        public bool UseTestEndPoint { get; set; }

        [Display(Order = 3)]
        [DataType(DataType.Password)]
        public string HashKey { get; set; }
    }
}