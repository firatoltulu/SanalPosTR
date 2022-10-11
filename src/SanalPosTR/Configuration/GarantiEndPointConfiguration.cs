using System;
using System.Collections.Generic;
using System.Text;

namespace SanalPosTR.Configuration
{
    internal class GarantiEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        public string ApiEndPoint => "VPServlet";

        public string SecureEndPointApi => "servlet/gt3dengine";

        public string SecureReturnEndPoint => "VPServlet";

        public string RefundEndPoint => "VPServlet";
    }
}
