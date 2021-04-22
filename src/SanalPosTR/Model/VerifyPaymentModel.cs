using System.Collections.Generic;

namespace SanalPosTR.Model
{
    public class VerifyPaymentModel
    {
        public OrderInfo Order { get; set; }

        public List<SimplePayAttribute> Attributes { get; set; } = new List<SimplePayAttribute>();

        public BankTypes SelectedBank { get; set; }

    }
}