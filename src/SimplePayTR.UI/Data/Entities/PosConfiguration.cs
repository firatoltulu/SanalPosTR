using LinqToDB.Mapping;
using SimplePayTR.Core;
using System;

namespace SimplePayTR.UI.Data.Entities
{
    public class PosConfiguration
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public bool Active { get; set; }

        public BankTypes BankType { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 1000)]
        public string Configuration { get; set; }

        public bool ForcePayRequest3D { get; set; }
    }
}