using SimplePayTR.Core.Providers.Est;

namespace SimplePayTR.Core.Configuration
{
    public interface ISimplePayConfiguration
    {
        ISimplePayConfiguration UseAkbank(NestPayConfiguration configuration);
        ISimplePayConfiguration UseAnadolubank(NestPayConfiguration configuration);
        ISimplePayConfiguration UseFinansBank(NestPayConfiguration configuration);
        ISimplePayConfiguration UseIsBank(NestPayConfiguration configuration);
        ISimplePayConfiguration UseTEB(NestPayConfiguration configuration);
        ISimplePayConfiguration UseYKB(YKBConfiguration configuration);
        ISimplePayConfiguration UseZiraat(NestPayConfiguration configuration);
    }
}