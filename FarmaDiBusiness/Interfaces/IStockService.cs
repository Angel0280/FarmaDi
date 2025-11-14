using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IStockService
    {
        Task<ServiceResponse<IEnumerable<Stock>>> GetAllAsync();
        Task<ServiceResponse<Stock>> GetByIdAsync(int id);
    }
}
