using SimplePayTR.Core.Helper;
using System.Collections.Generic;

namespace SimplePayTR.Core.Model
{
    public class PaymentModel
    {
        public CreditCardInfo CreditCard { get; set; }

        public OrderInfo Order { get; set; }

        public bool Use3DSecure { get; set; }

        public PaymentModel Clone()
        {
            var target = new PaymentModel();
            new MapperOptimized().Copy(this, target);
            return target;
        }

        public List<SimplePayAttribute> Attributes { get; set; } = new List<SimplePayAttribute>();
    }
}