using System.Collections.Generic;

namespace SimplePayTR
{
    public class OrderInfo
    {
        public string OrderId { get; set; }

        public string UserId { get; set; }

        public string EMail { get; set; }

        public decimal Total { get; set; }

        public decimal Comission { get; set; }

        public string CurrencyCode { get; set; } 


        public int? Installment { get; set; }


    }

    public partial class SimplePayAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}