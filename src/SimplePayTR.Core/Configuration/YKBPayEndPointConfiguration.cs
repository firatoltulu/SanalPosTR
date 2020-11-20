using System;
using System.Collections.Generic;
using System.Text;

namespace SimplePayTR.Core.Configuration
{
    internal class YKBEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        public string ApiEndPoint => "PosnetWebService/XML";

        public string SecureEndPointApi => "3DSWebService/YKBPaymentService";

        public string SecureReturnEndPoint => "PosnetWebService/XML";

        public string RefundEndPoint => "PosnetWebService/XML";
    }
}
