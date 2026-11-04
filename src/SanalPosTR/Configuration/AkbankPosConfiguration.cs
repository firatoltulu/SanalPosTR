using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{
    /// <summary>
    /// Akbank yeni sanal POS altyapısı (sanalpos.akbank.com). Eski NestPay altyapısı <see cref="BankTypes.Akbank"/> ile çalışır.
    /// </summary>
    public class AkbankPosConfiguration : IProviderConfiguration, I3DConfiguration
    {
        [Display(Order = 0)]
        public string MerchantSafeId { get; set; }

        [Display(Order = 1)]
        public string TerminalSafeId { get; set; }

        [Display(Order = 2)]
        [DataType(DataType.Password)]
        public string SecretKey { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        [Display(Order = 3)]
        public bool UseTestEndPoint { get; set; }
    }
}
