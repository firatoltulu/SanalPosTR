using LinqToDB.Mapping;
using SimplePayTR.Core;
using System;

namespace SimplePayTR.UI.Data.Entities
{
    public class PosBinNumber
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public bool Active { get; set; }

        public BankTypes BankType { get; set; }

        public string Number { get; set; }
    }
}