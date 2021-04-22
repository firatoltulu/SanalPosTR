using System;
using System.Collections.Generic;
using System.Text;

namespace SanalPosTR.Configuration
{
    internal class NestPayEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        public string ApiEndPoint => "fim/api";

        public string SecureEndPointApi => "fim/est3Dgate";

        public string SecureReturnEndPoint => "fim/api";

        public string RefundEndPoint => "fim/api";
    }
}
