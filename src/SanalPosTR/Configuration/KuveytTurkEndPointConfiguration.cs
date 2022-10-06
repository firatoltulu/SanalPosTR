using System;
using System.Collections.Generic;
using System.Text;

namespace SanalPosTR.Configuration
{
    internal class KuveytTurkEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        public string ApiEndPoint => "PosnetWebService/XML";

        public string SecureEndPointApi => "3DSWebService/YKBPaymentService";

        public string SecureReturnEndPoint => "PosnetWebService/XML";

        public string RefundEndPoint => "PosnetWebService/XML";
    }
}
