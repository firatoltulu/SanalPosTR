using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public class Request
    {
        public string Url { get; set; }

        public Dictionary<string, object> Accounts { get; set; }
        
        public RequestPos Pos { get; set; }

        public bool Is3D { get; set; }

        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }

        public int Id { get; set; }

    }

    public class RequestPos {

        public RequestPos()
        {
            Installment = 0;
        }

        public string CardNumber { get; set; }

        public string ExpireDate { get; set; }

        public string CVV2 { get; set; }

        public string Hash { get; set; }

        public string FullName { get; set; }

        public string EMail { get; set; }

        public string Ip { get; set; }

        public decimal Total { get; set; }

        public decimal Comission { get; set; }

        public int Installment { get; set; }

        /// <summary>
        /// TransactionId
        /// </summary>
        public string ProcessId { get; set; }

        public string UserId { get; set; }

        public string Extra { get; set; }

        public int BankId { get; set; }

        public string SpecialField1 { get; set; }

        public string SpecialField2 { get; set; }
    }
}
