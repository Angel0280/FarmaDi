using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SupplierDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;


namespace FarmaDiBusiness.Interfaces
{
    public interface ISupplierService
    {
        Task<ServiceResponse<Suppliers>> AddAsync(AddSupplierDto newSupplier);
        Task<ServiceResponse<IEnumerable<Suppliers>>> GetAllAsync();
        Task<ServiceResponse<Suppliers>> GetByIdAsync(int id);
        Task<ServiceResponse<Suppliers>> UpdateAsync(int id, UpdateSupplierDto Supplier);
        Task<ServiceResponse<Suppliers>> GetByNameAsync(string name);
        Task<ServiceResponse<Suppliers>> SetStateAsync(int id, bool state);
    }
}
