using SimplePayTR.UI.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimplePayTR.UI.Data
{
    public interface IDataServices
    {
        Task<List<PosConfiguration>> GetPosConfigurationsAsync();
        Task InsertPaySessionAsync(PaySession paySession);
        Task UpdatePaySessionAsync(PaySession paySession);
    }
}
