using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;

namespace FarmaDiBusiness.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseService(IPurchaseRepository purchaseRepository, ISupplierService supplierService, IProductService productService)
        {
            _purchaseRepository = purchaseRepository;
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto)
        {
            try
            {
                // 1. Validación perimetral de existencia de Proveedor
                var existingSupplier = await _supplierService.GetByIdAsync(dto.SupplierId);
                if (existingSupplier?.Data == null)
                {
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, // CORREGIDO: Semántica correcta para recurso no encontrado
                        Message = "No se encontró un proveedor con el id proporcionado"
                    };
                }

                // 2. CORREGIDO: Validación perimetral de existencia de cada Producto en el detalle
                foreach (var detailDto in dto.Details)
                {
                    var existingProduct = await _productService.GetByIdAsync(detailDto.ProductId);
                    if (existingProduct?.Data == null)
                    {
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"No se encontró el producto con el id {detailDto.ProductId} especificado en el detalle"
                        };
                    }
                }

                // 3. Mapeo seguro DTO -> Entidad Maestra
                var purchase = new Purchase
                {
                    SupplierId = dto.SupplierId,
                    UserId = dto.UserId,
                    Observation = dto.Observation,
                    RegisteredDate = DateTime.Now
                };

                // 4. Mapeo seguro DTO -> Colección de Entidades de Detalle
                var purchaseDetail = dto.Details.Select(dt => new PurchaseDetails
                {
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity,
                    UnitPrice = dt.UnitPrice,
                    BatchNumber = dt.BatchNumber,
                    ExpirationDate = dt.ExpirationDate,
                    ManufacturingDate = dt.ManufacturingDate
                }).ToList();

                // 5. Inserción atómica en el repositorio transaccional
                var repoResponse = await _purchaseRepository.InserAsync(purchase, purchaseDetail); // CORREGIDO: Casing camelCase

                // 6. Evaluación y mapeo de la respuesta de persistencia
                if (repoResponse.OperationStatusCode == 0)
                {
                    var dataResponse = new PurchaseResponseDto
                    {
                        Id = repoResponse.Data!.Master.PurchaseId,
                        SupplierId = repoResponse.Data.Master.SupplierId,
                        UserId = repoResponse.Data.Master.UserId,
                        total = repoResponse.Data.Master.Total,
                        Observation = repoResponse.Data.Master.Observation,
                        PurchaseDate = repoResponse.Data.Master.RegisteredDate,
                        PurchaseNum = repoResponse.Data.Master.PurchaseNum,

                        Details = repoResponse.Data.Details.Select(dt => new PurchaseDetailsResponseDto
                        {
                            Id = dt.Id,
                            PurchaseId = dt.PurchaseId,
                            ProductId = dt.ProductId,
                            Quantity = dt.Quantity,
                            UnitPrice = dt.UnitPrice,
                            BatchNumber = dt.BatchNumber,
                            ExpirationDate = dt.ExpirationDate,
                            ManufacturingDate = dt.ManufacturingDate,
                            BatchId = dt.BatchId,
                            TotalPrice = dt.TotalPrice,
                            RegisteredDate = dt.RegisteredDate
                        }).ToList()
                    };

                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Compra registrada exitosamente"
                    };
                }

                // 7. CORREGIDO: Mapeo inteligente de códigos operacionales de SQL Server hacia la API
                switch (repoResponse.OperationStatusCode)
                {
                    case 50009: // Recurso no encontrado en procedimientos almacenados
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontraron los recursos relacionales solicitados en la base de datos"
                        };

                    case 50003: // Conflicto transaccional o datos duplicados
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = repoResponse.Message ?? "Conflicto de datos o duplicidad al procesar la compra"
                        };

                    default:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Error en el registro de la compra"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el servicio transaccional de compras."
                };
            }
        }
    }
}