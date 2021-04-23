using System.Collections.Generic;

namespace SanalPosTR.Model
{
    public class VerifyPaymentModel
    {
        public OrderInfo Order { get; set; }

        public List<SanalPosTRAttribute> Attributes { get; set; } = new List<SanalPosTRAttribute>();

        public BankTypes SelectedBank { get; set; }

    }
}