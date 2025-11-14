using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IInventoryService
    {
        Task<ServiceResponse<IEnumerable<Inventory>>> GetAllAsync();
        Task<ServiceResponse<Inventory>> GetByIdAsync(int id);
    }
}
