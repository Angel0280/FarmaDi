using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface ISaleService
    {
        Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto);
        Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id);
        Task<ServiceResponse<PagedSaleResult>> GetSalesPagedAsync(int pageNumber, int pageSize);
    }
}

