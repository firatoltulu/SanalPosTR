using SimplePayTR.Core.Helper;
using System;
using System.Collections.Generic;

namespace SimplePayTR.Core.Model
{
    public class PaymentModel
    {
        public PaymentModel()
        {
        }
        public CreditCardInfo CreditCard { get; set; }

        public OrderInfo Order { get; set; }

        public bool Use3DSecure { get; set; }

        public PaymentModel Clone()
        {
            return ObjectCopier.Clone(this);
        }

        public List<SimplePayAttribute> Attributes { get; set; } = new List<SimplePayAttribute>();

        public string SessionId { get; set; }
    }
}