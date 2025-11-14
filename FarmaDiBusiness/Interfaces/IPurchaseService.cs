using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IPurchaseService
    {
        Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto);

    }
}
