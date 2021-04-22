using SanalPosTR;
using SanalPosTR.Model;
using System;

namespace SanalPosTR.Playground.Models
{
    public class PaymentActionModel : PaymentModel
    {
        public BankTypes SelectedBank { get; set; }


        public Guid PosConfigurationId { get; set; }
    }
}