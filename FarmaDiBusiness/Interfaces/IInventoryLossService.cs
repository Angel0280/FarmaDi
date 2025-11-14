using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IInventoryLossService
    {
        Task<ServiceResponse<IEnumerable<InventoryLoss>>> GetAllAsync();
        Task<ServiceResponse<InventoryLoss>> GetByIdAsync(int id);
        Task<ServiceResponse<InventoryLoss>> AddAsync(AddInventoryLossDto newInventoryLoss);

        //Task<ServiceResponse<ProductBatches>> GetBatchByIdAsync(int id);


    }
}
