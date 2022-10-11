using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{
    public class KuveytTurkConfiguration : IProviderConfiguration, I3DConfiguration
    {
        [Display(Order = 0)]
        public string MerchantId { get; set; }

        [Display(Order = 1)]
        public string CustomerId { get; set; }

        [Display(Order = 3)]
        public string UserName { get; set; }

        [Display(Order = 3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        [Display(Order = 5)]
        public bool UseTestEndPoint { get; set; }
    }
}