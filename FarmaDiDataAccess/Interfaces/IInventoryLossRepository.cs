using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IInventoryLossRepository
    {
        Task<RepositoryResponse<InventoryLoss>> AddAsync(InventoryLoss inventoryLoss);
       
       Task<RepositoryResponse<IEnumerable<InventoryLoss>>> GetAllAsync();

       
        Task<RepositoryResponse<InventoryLoss>> GetByIdAsync(int id);


        //  Task<RepositoryResponse<Brands>> UpdateAsync(int id, Brands brands);


        //  Task<RepositoryResponse<Brands>> GetByNameAsync(string name);


        //  Task<RepositoryResponse<Brands>> SetStateAsync(int id, bool state);

        


        //  Task<RepositoryResponse<Users>> GetByIdAsync (int id );
    }
}
