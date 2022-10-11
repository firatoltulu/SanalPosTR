namespace SanalPosTR.Model
{
    public class OrderInfo
    {
        public string IP { get; set; }
        public string OrderId { get; set; }

        public string UserId { get; set; }

        public string EMail { get; set; }

        public decimal Total { get; set; }

        public decimal Comission { get; set; }

        public string CurrencyCode { get; set; }

        public int? Installment { get; set; }

        public string CustomerId { get; set; }
    }

    public partial class SanalPosTRAttribute
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }
}