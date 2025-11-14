using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IProductBatchesRepository
    {
        Task<RepositoryResponse<IEnumerable<ProductBatches>>> GetAllAsync();
        Task<RepositoryResponse<ProductBatches>> GetByIdAsync(int id);
    }
}
