using FarmaDiBusiness.DTOs.Inventory;
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
        Task<ServiceResponse<InventoryDashboardDto>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte);


    }
}
