using System.Collections.Generic;

namespace SimplePayTR.Core.Model
{
    public class VerifyPaymentModel
    {
        public OrderInfo Order { get; set; }

        public List<SimplePayAttribute> Attributes { get; set; } = new List<SimplePayAttribute>();

    }
}