namespace SanalPosTR.Configuration
{
    public interface IConfigurationService
    {
        IConfigurationService SetSuccessReturnUrl(string url);

        IConfigurationService SetFailReturnUrl(string url);

        IConfigurationService UseAkbank(NestPayConfiguration configuration);

        IConfigurationService UseAnadolubank(NestPayConfiguration configuration);

        IConfigurationService UseFinansBank(NestPayConfiguration configuration);

        IConfigurationService UseFromJSON(BankTypes bankTypes, string jsonValue);

        IConfigurationService UseIsBank(NestPayConfiguration configuration);

        IConfigurationService UseTEB(NestPayConfiguration configuration);

        IConfigurationService UseYKB(YKBConfiguration configuration);

        IConfigurationService UseZiraat(NestPayConfiguration configuration);

        IConfigurationService UseKuveytTurk(KuveytTurkConfiguration configuration);
        IConfigurationService SetBankEnvironment(BankTypes bankTypes, bool useTest);
    }
}