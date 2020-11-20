using SimplePayTR.Core;
using SimplePayTR.Core.Model;
using System;

namespace SimplePayTR.UI.Models
{
    public class PaymentActionModel : PaymentModel
    {
        public BankTypes SelectedBank { get; set; }

        public string SessionId { get; set; }

        public Guid PosConfigurationId { get; set; }
    }
}