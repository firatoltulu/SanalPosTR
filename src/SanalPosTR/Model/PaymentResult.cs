namespace SanalPosTR
{


    public class PaymentResult
    {
        public bool Status { get; set; }

        public string ServerResponseRaw { get; set; }

        public string OrderContentRaw { get; set; }

        public string ProvisionNumber { get; set; }

        public string ReferanceNumber { get; set; }

        public string Error { get; set; }

        public string ErrorCode { get; set; }

        public bool IsRedirectContent { get; set; }

        public PaymentResult()
        {
        }

        public PaymentResult(bool status, string error)
        {
            Error = error;
            Status = status;
        }
    }
}