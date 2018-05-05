using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public class Result
    {
        public bool Status { get; set; }

        public string ResultContent { get; set; }

        public string RequestContent { get; set; }

        public string ProvisionNumber { get; set; }

        public string ReferanceNumber { get; set; }

        public string ProcessId { get; set; }

        public string Error { get; set; }

        public string ErrorCode { get; set; }

        public NetworkType iNetworkType { get; set; }

        public Result() { }

        public Result(bool status, string error)
        {
            Error = error;
            Status = status;
        }

        public Request RequestData { get; set; }

    }

   
}
