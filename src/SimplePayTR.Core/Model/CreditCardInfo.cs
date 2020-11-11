using System;
using System.Collections.Generic;
using System.Text;

namespace SimplePayTR.Core.Model
{
    public class CreditCardInfo
    {
        public string CardNumber { get; set; }

        public string ExpireDate { get; set; }

        public string CVV2 { get; set; }

        public string CardHolderName { get; set; }

        public List<SimplePayAttribute> Attributes { get; set; } = new List<SimplePayAttribute>();
    }
}
