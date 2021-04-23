using SanalPosTR.Helper;
using System;
using System.Collections.Generic;

namespace SanalPosTR.Model
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

        public List<SanalPosTRAttribute> Attributes { get; set; } = new List<SanalPosTRAttribute>();

        public string SessionId { get; set; }
    }
}