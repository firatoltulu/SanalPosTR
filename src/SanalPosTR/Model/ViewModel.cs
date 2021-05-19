using SanalPosTR.Configuration;

namespace SanalPosTR.Model
{
    public class ViewModel : PaymentModel
    {
        public IProviderConfiguration Configuration { get; set; }

        public Refund Refund { get; set; }

        public IEnvironmentConfiguration Environment { get; set; }
    }
}