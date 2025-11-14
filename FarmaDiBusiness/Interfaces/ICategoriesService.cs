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
    public interface ICategoriesService
    {
        Task<ServiceResponse<Categories>> AddAsync(AddCategoryDto newcategory);
        Task<ServiceResponse<IEnumerable<Categories>>> GetAllAsync();
        Task<ServiceResponse<Categories>> GetByIdAsync(int id);
        Task<ServiceResponse<Categories>> UpdateAsync(int id, UpdateCategoryDto category);
        Task<ServiceResponse<Categories>> GetByNameAsync(string name);
        Task<ServiceResponse<Categories>> SetStateAsync(int id, bool state);
    }
}
