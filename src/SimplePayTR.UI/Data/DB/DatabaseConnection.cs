using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using SimplePayTR.UI.Data.Entities;

namespace SimplePayTR.UI.Data.DB
{
    public class DatabaseConnection : DataConnection
    {
        public DatabaseConnection(LinqToDbConnectionOptions<DatabaseConnection> options)
        : base(options)
        {
        }

        public ITable<PaySession> PaySessions => GetTable<PaySession>();
        public ITable<PosConfiguration> PosConfigurations => GetTable<PosConfiguration>();
        public ITable<PosBinNumber> PosBinNumbers => GetTable<PosBinNumber>();

        public ITable<PosInstallment> PosInstallments => GetTable<PosInstallment>();
    }
}