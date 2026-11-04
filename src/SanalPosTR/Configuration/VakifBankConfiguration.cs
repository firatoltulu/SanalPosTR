using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{
    /// <summary>
    /// VakıfBank VPOS 7/24. 3D ödemeler 3DS Metodu ile alınır; üye işyerinin 3DS yetkisini bankaya açtırması gerekir.
    /// </summary>
    public class VakifBankConfiguration : IProviderConfiguration, I3DConfiguration
    {
        [Display(Order = 0)]
        public string MerchantId { get; set; }

        [Display(Order = 1)]
        public string TerminalNo { get; set; }

        [Display(Order = 2)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        [Display(Order = 3)]
        public bool UseTestEndPoint { get; set; }
    }
}
