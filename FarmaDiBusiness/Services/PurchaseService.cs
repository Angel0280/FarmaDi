// FarmaDiBusiness.Services.PurchaseService
using FarmaDiBusiness.DTOs.PurchaseDto; // Asegúrate de tener los DTOs
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;

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

    // CAMBIO: La firma ahora devuelve PurchaseResponseDto
    public async Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto)
    {
        try
        {
            // --- (Validaciones de Proveedor y Producto - Están correctas) ---
            var existingSupplier = await _supplierService.GetByIdAsync(dto.SupplierId);
            if (existingSupplier.Data == null)
            {
                return new ServiceResponse<PurchaseResponseDto> // <-- Tipo de respuesta actualizado
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorValidation,
                    Message = "No se encontró un proveedor con el id proporcionado"
                };
            }
            // ... (Validación de productos omitida por brevedad, está bien) ...


            // --- (Mapeo DTO -> Entidad - Está correcto) ---
            var purchase = new Purchase
            {
                SupplierId = dto.SupplierId,
                UserId = dto.UserId,
                Observation = dto.Observation,
                RegisteredDate = DateTime.Now,
               // PurchaseNum = dto.PurchaseNum
            };

            var purchaseDetail = dto.Details.Select(dt => new PurchaseDetails
            {
                ProductId = dt.ProductId,
                Quantity = dt.Quantity,
                UnitPrice = dt.UnitPrice,
                BatchNumber = dt.BatchNumber,
                ExpirationDate = dt.ExpirationDate,
                ManufacturingDate = dt.ManufacturingDate
            });

            // --- (Llamada al Repositorio - Está correcto) ---
            var RepoResponse = await _purchaseRepository.InserAsync(purchase, purchaseDetail);

            // --- (Respuesta del Servicio - LÓGICA DE MAPEO AÑADIDA) ---
            if (RepoResponse.OperationStatusCode == 0)
            {
                // ¡AQUÍ HACEMOS EL MAPEO!
                // (Es el código que tenías en el controlador)
                var dataResponse = new PurchaseResponseDto();
                dataResponse.Id = RepoResponse.Data!.Master.PurchaseId;
                dataResponse.SupplierId = RepoResponse.Data!.Master.SupplierId;
                dataResponse.UserId = RepoResponse.Data!.Master.UserId; // Corregido
                dataResponse.total = RepoResponse.Data!.Master.Total;
                dataResponse.Observation = RepoResponse.Data!.Master.Observation;
                dataResponse.PurchaseDate = RepoResponse.Data!.Master.RegisteredDate;
                dataResponse.PurchaseNum = RepoResponse.Data!.Master.PurchaseNum;

                dataResponse.Details = RepoResponse.Data!.Details.Select(dt => new PurchaseDetailsResponseDto
                {
                    Id = dt.Id,
                    PurchaseId = dt.PurchaseId,
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity,
                    UnitPrice = dt.UnitPrice,
                    BatchNumber = dt.BatchNumber, // Asegúrate que la entidad 'dt' tenga esto
                    ExpirationDate = dt.ExpirationDate, // Asegúrate que la entidad 'dt' tenga esto
                    ManufacturingDate = dt.ManufacturingDate, // Asegúrate que la entidad 'dt' tenga esto
                    BatchId = dt.BatchId,
                    TotalPrice = dt.TotalPrice,
                    RegisteredDate = dt.RegisteredDate
                }).ToList();


                // Devolvemos el DTO mapeado
                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = dataResponse, // <-- DTO en lugar de Entidad
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Compra registrada exitosamente"
                };
            }
            else
            {
                // Error
                return new ServiceResponse<PurchaseResponseDto> // <-- Tipo de respuesta actualizado
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = RepoResponse.Message ?? "Error en el registro de la compra"
                };
            }
        }
        catch (Exception ex)
        {
            // TODO: Log ex
            return new ServiceResponse<PurchaseResponseDto> // <-- Tipo de respuesta actualizado
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = "Ocurrió un error inesperado en el servicio."
            };
        }
    }
}