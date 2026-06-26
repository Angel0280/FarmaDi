using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using static FarmaDiBusiness.DTOs.Inventory.InventoryDashboardDto;

namespace FarmaDiBusiness.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Inventory>>> GetAllAsync()
        {
            try
            {
                // CORREGIDO: Todo el flujo envuelto en try-catch para evitar crasheos si el repositorio falla
                var result = await _inventoryRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Inventory>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<IEnumerable<Inventory>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Inventory>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Antes mapeaba a NoData por error
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Inventory>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar el inventario"
                };
            }
        }

        public async Task<ServiceResponse<Inventory>> GetByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Movido de forma segura dentro del bloque try
                var result = await _inventoryRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Inventory>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<Inventory>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No se encontró ningún registro que coincida con los parámetros de búsqueda"
                        };

                    default:
                        return new ServiceResponse<Inventory>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<Inventory>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar el registro de inventario"
                };
            }
        }



        public async Task<ServiceResponse<InventoryDashboardDto>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte)
        {
            var result = await _inventoryRepository.GetDashboardAsync(page, limit, categoryId, estado, brandId, supplierId, fechaCorte);

            if (result.OperationStatusCode == 0 && result.Data != null)
            {
                // Mapeo explícito de la Entidad al DTO
                var dto = new InventoryDashboardDto
                {
                    Summary = new InventorySummaryDto
                    {
                        TotalProductos = result.Data.Summary.TotalProductos,
                        StockBajo = result.Data.Summary.StockBajo,
                        Agotados = result.Data.Summary.Agotados,
                        ValorInventario = result.Data.Summary.ValorInventario
                    },
                    Items = result.Data.Items.Select(i => new InventoryItemDto
                    {
                        ProductId = i.ProductId,
                        Producto = i.Producto,
                        NombreGenerico = i.NombreGenerico,
                        CategoryId = i.CategoryId,
                        Categoria = i.Categoria,
                        PresentationId = i.PresentationId,
                        SupplierId = i.SupplierId,
                        BrandId = i.BrandId,
                        Isactive = i.Isactive,
                        Precio = i.Precio,
                        PrecioCosto = i.PrecioCosto,
                        StockCritico = i.StockCritico,
                        Existencia = i.Existencia,
                        CantidadVencida = i.CantidadVencida,
                        ValorProducto = i.ValorProducto,
                        Estado = i.Estado
                    }).ToList(),
                    Batches = result.Data.Batches.Select(b => new InventoryBatchInfoDto
                    {
                        BatchId = b.BatchId,
                        NumeroLote = b.NumeroLote,
                        FechaFabricacion = b.FechaFabricacion,
                        FechaVencimiento = b.FechaVencimiento,
                        CantidadOriginal = b.CantidadOriginal,
                        CantidadDisponible = b.CantidadDisponible,
                        ProductId = b.ProductId,
                        FechaRegistro = b.FechaRegistro,
                        Activo = b.Activo,
                        StockId = b.StockId,
                        FechaEntradaStock = b.FechaEntradaStock,
                        EstadoLote = b.EstadoLote
                    }).ToList()
                };

                return new ServiceResponse<InventoryDashboardDto>
                {
                    Data = dto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            return new ServiceResponse<InventoryDashboardDto>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Ocurrió un error inesperado al obtener el dashboard"
            };
        }

    }
}