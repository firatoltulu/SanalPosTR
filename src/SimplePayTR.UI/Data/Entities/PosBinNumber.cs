using LinqToDB.Mapping;
using SanalPosTR;
using System;

namespace SanalPosTR.Playground.Data.Entities
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