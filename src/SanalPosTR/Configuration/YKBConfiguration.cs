using SanalPosTR.Configuration;
using System.ComponentModel.DataAnnotations;

namespace SanalPosTR.Configuration
{
    public class YKBConfiguration : IProviderConfiguration, I3DConfiguration
    {

        [Display(Order =0)]
        /// <summary>
        /// Üye iş yeri no
        /// </summary>
        public string MerchantId { get; set; }

        [Display(Order = 1)]
        /// <summary>
        /// Terminal no
        /// </summary>
        public string TerminalId { get; set; }

        [Display(Order = 2)]
        /// <summary>
        /// Posnet no
        /// </summary>
        public string PosnetId { get; set; }

        [Display(Order = 3)]
        public string UserName { get; set; }

        [Display(Order = 3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string SiteSuccessUrl { get; set; }

        public string SiteFailUrl { get; set; }

        [Display(Order = 5)]
        public bool UseTestEndPoint { get; set; }

        [Display(Order = 4)]
        [DataType(DataType.Password)]
        public string HashKey { get; set; }
    }
}