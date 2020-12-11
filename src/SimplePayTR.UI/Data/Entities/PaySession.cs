using LinqToDB.Mapping;
using SimplePayTR.Core;
using System;

namespace SimplePayTR.UI.Data.Entities
{
    public class PaySession
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 20)]
        public string CardNumber { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 60)]
        public string CardHolderName { get; set; }

        public bool Status { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 24)]
        public string OrderId { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 60)]
        public string UserId { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 90)]
        public string CustomerId { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 350)]
        public string Error { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 50)]
        public string EMail { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 20)]
        public string ErrorCode { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 10)]
        public string ProvisionNumber { get; set; }

        [Column(DataType = LinqToDB.DataType.VarChar, Length = 32)]
        public string ReferanceNumber { get; set; }

        public bool UseSecure3D { get; set; }

        public DateTime CreateOn { get; set; }

        public BankTypes Bank { get; set; }

        public Guid PosConfigurationId { get; set; }

        public decimal Amount { get; set; }

        public int? Installment { get; set; }

        public bool TransferStatus { get; set; } = false;
    }
}