namespace SimplePayTR.Core.Configuration
{
    public interface ISimplePayConfiguration
    {
        ISimplePayConfiguration SetSuccessReturnUrl(string url);

        ISimplePayConfiguration SetFailReturnUrl(string url);

        ISimplePayConfiguration UseAkbank(NestPayConfiguration configuration);

        ISimplePayConfiguration UseAnadolubank(NestPayConfiguration configuration);

        ISimplePayConfiguration UseFinansBank(NestPayConfiguration configuration);

        ISimplePayConfiguration UseFromJSON(BankTypes bankTypes, string jsonValue);

        ISimplePayConfiguration UseIsBank(NestPayConfiguration configuration);

        ISimplePayConfiguration UseTEB(NestPayConfiguration configuration);

        ISimplePayConfiguration UseYKB(YKBConfiguration configuration);

        ISimplePayConfiguration UseZiraat(NestPayConfiguration configuration);
    }
}