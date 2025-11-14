using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public  interface IProductService
    {
        Task<ServiceResponse<Products>> AddAsync(AddProductDto newproduct);
        Task<ServiceResponse<IEnumerable<Products>>> GetAllAsync();
        Task<ServiceResponse<Products>> GetByIdAsync(int id);
        Task<ServiceResponse<Products>> UpdateAsync(int id, UpdateProductDto product);
        Task<ServiceResponse<Products>> GetByNameAsync(string name);
        Task<ServiceResponse<Products>> SetStateAsync(int id, bool state);
    }
}
