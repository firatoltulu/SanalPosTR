using LinqToDB;
using SimplePayTR.UI.Data.DB;
using SimplePayTR.UI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimplePayTR.UI.Data
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
    }
}