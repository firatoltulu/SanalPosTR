namespace SanalPosTR.Configuration
{
    internal class AkbankPosEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        // 3D formu Payment API'den farklı bir host'a gidiyor
        public string GatewayBaseUrl { get; set; }

        public string ApiEndPoint => "api/v1/payment/virtualpos/transaction/process";

        public string SecureEndPointApi => "securepay";

        public string SecureReturnEndPoint => ApiEndPoint;

        public string RefundEndPoint => ApiEndPoint;
    }
}
