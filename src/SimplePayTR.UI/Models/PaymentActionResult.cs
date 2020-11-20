namespace SimplePayTR.UI.Models
{
    public class PaymentActionResult
    {
        public bool Status { get; set; }

        public object Data { get; set; }

        public string Error { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class PaymentActionDataResult
    {
        public bool IsRedirect { get; set; }

        public string Content { get; set; }
    }
}