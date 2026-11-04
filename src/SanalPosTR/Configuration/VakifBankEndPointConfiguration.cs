namespace SanalPosTR.Configuration
{
    internal class VakifBankEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        // Canlıda enrollment (inbound.apigateway) ile provizyon (apigw) farklı host'larda
        public string EnrollmentBaseUrl { get; set; }

        public string EnrollmentEndPoint => "threeDGateway/ProcessEnrollment";

        public string ApiEndPoint => "virtualPos/Vposreq";

        public string SecureEndPointApi => EnrollmentEndPoint;

        public string SecureReturnEndPoint => ApiEndPoint;

        public string RefundEndPoint => ApiEndPoint;
    }
}
