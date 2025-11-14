using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IInventoryRepository
    {
        Task<RepositoryResponse<IEnumerable<Inventory>>> GetAllAsync();
        Task<RepositoryResponse<Inventory>> GetByIdAsync(int id);
    }
}
