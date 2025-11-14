using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IProductsRepository
    {
        Task<RepositoryResponse<Products>> AddAsync(Products Products);
        
        Task<RepositoryResponse<IEnumerable<Products>>> GetAllAsync();

 
        Task<RepositoryResponse<Products>> GetByIdAsync(int id);

        Task<RepositoryResponse<Products>> UpdateAsync(int id, Products Products);

       
        Task<RepositoryResponse<Products>> GetByNameAsync(string name);


        Task<RepositoryResponse<Products>> SetStateAsync(int id, bool state);
    }
}
