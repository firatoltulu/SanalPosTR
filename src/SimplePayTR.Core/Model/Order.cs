using System.Collections.Generic;

namespace SimplePayTR
{
    public class Order
    {
        public string OrderId { get; set; }

        public string UserId { get; set; }

        public string EMail { get; set; }

        public decimal Total { get; set; }

        public decimal Comission { get; set; }

        public int Installment { get; set; }

        public bool Use3DSecure { get; set; }

        public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    }

    public partial class Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}