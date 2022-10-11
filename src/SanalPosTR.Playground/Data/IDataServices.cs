using SanalPosTR.Playground.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanalPosTR.Playground.Data
{
    public interface IDataServices
    {
        Task<List<PosConfiguration>> GetPosConfigurationsAsync();

        Task InsertPaySessionAsync(PaySession paySession);

        Task UpdatePaySessionAsync(PaySession paySession);

        Task<IEnumerable<PosInstallment>> GetPosInstallments(string binNumber);

        Task<PaySession> GetPaySession(string orderId);
    }
}