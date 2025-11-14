using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FarmaDiDataAccess.Interfaces
{
    public interface ISuppliersRepository
    {

  
        Task<RepositoryResponse<Suppliers>> AddAsync(Suppliers Suppliers);
      
        Task<RepositoryResponse<IEnumerable<Suppliers>>> GetAllAsync();

   
        Task<RepositoryResponse<Suppliers>> GetByIdAsync(int id);

        
        Task<RepositoryResponse<Suppliers>> UpdateAsync(int id, Suppliers Suppliers);

        Task<RepositoryResponse<Suppliers>> GetByNameAsync(string name);

        
        Task<RepositoryResponse<Suppliers>> SetStateAsync(int id, bool state);
    }
}
