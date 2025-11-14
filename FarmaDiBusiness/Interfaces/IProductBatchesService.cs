using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IProductBatchesService
    {
        Task<ServiceResponse<IEnumerable<ProductBatches>>> GetAllAsync();
        Task<ServiceResponse<ProductBatches>> GetByIdAsync(int id);
    }
}
