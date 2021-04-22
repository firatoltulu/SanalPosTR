using LinqToDB.Mapping;
using SanalPosTR;
using System;

namespace SanalPosTR.Playground.Data.Entities
{
    public class PosInstallment
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public bool Active { get; set; }

        public BankTypes BankType { get; set; }

        public int Installment { get; set; }

        public decimal Interest { get; set; }

        public byte DisplayOrder { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 30)]
        public string ExtraInfo { get; set; }
    }
}