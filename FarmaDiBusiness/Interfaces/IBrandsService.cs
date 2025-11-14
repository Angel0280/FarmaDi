using FarmaDiBusiness.DTOs;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IBrandsService
    {
        Task<ServiceResponse<Brands>> AddAsync(AddBrandDto newbrand);
        Task<ServiceResponse<IEnumerable<Brands>>> GetAllAsync();
        Task<ServiceResponse<Brands>> GetByIdAsync(int id);
        Task<ServiceResponse<Brands>> UpdateAsync(int id, UpdateBrandDto brand);
        Task<ServiceResponse<Brands>> GetByNameAsync(string name);
        Task<ServiceResponse<Brands>> SetStateAsync(int id, bool state);
    }
}
