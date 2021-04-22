using LinqToDB;
using SanalPosTR;
using SanalPosTR.Playground.Data.DB;
using SanalPosTR.Playground.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanalPosTR.Playground.Data
{
    public class DataServices : IDataServices
    {
        private readonly DatabaseConnection _databaseConnection;

        public DataServices(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<List<PosConfiguration>> GetPosConfigurationsAsync()
        {
            return await _databaseConnection.PosConfigurations.ToListAsync();
        }

        public async Task<List<PaySession>> GetPaySessionsAsync()
        {
            return await _databaseConnection.PaySessions.ToListAsync();
        }

        public async Task InsertPaySessionAsync(PaySession paySession)
        {
            await _databaseConnection.InsertAsync<PaySession>(paySession);
        }

        public async Task UpdatePaySessionAsync(PaySession paySession)
        {
            await _databaseConnection.UpdateAsync(paySession);
        }

        public async Task<IEnumerable<PosInstallment>> GetPosInstallments(string binNumber)
        {
            BankTypes bankType;

            var currentBank = _databaseConnection.PosBinNumbers.Where(whr => whr.Number == binNumber && whr.Active == true).FirstOrDefault();
            if (currentBank == null)
            {
                bankType = _databaseConnection.PosConfigurations.Where(t => t.UseDefault == true && t.Active == true).Select(v => v.BankType).FirstOrDefault();
                return await _databaseConnection.PosInstallments.Where(whr => whr.BankType == bankType && whr.Installment == 1).OrderBy(v => v.DisplayOrder).ToListAsync();
            }
            else
                bankType = currentBank.BankType;

            return await _databaseConnection.PosInstallments.Where(whr => whr.BankType == bankType).OrderBy(v => v.DisplayOrder).OrderBy(v => v.DisplayOrder).ToListAsync();
        }

        public async Task<PaySession> GetPaySession(string orderId)
        {
            return await _databaseConnection.PaySessions.FirstOrDefaultAsync(v => v.OrderId == orderId);
        }
    }
}