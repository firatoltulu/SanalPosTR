using SimplePayTR.Core.Providers.Est;

namespace SimplePayTR.Core.Configuration
{
    public interface ISimplePayConfiguration
    {
        ISimplePayConfiguration UseEst(EstConfiguration estConfiguration);
    }
}