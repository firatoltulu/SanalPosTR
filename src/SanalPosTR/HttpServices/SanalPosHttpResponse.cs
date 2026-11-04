using System;
using System.Collections.Generic;

namespace SanalPosTR
{
    public class SanalPosHttpResponse
    {
        public int StatusCode { get; set; }

        public bool IsSuccess { get; set; }

        public string Body { get; set; }

        // İmza doğrulaması baytlar üzerinden yapılmalı: Body, yanıtın bildirdiği charset ile çözülmüş haldir.
        public byte[] RawBody { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
