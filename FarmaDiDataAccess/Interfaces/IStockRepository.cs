using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IStockRepository
    {
        Task<RepositoryResponse<IEnumerable<Stock>>> GetAllAsync();
        Task<RepositoryResponse<Stock>> GetByIdAsync(int id);
    }
}
