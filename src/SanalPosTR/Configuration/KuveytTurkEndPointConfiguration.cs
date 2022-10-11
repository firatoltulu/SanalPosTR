using System;
using System.Collections.Generic;
using System.Text;

namespace SanalPosTR.Configuration
{
    internal class KuveytTurkEndPointConfiguration : IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        public string ApiEndPoint { get; set; } = "boa.virtualpos.services/Home/ThreeDModelPayGate";

        public string SecureEndPointApi { get; set; } = "boa.virtualpos.services/Home/ThreeDModelPayGate";

        public string SecureReturnEndPoint { get; set; } = "PosnetWebService/XML";

        public string RefundEndPoint => "PosnetWebService/XML";
    }
}
