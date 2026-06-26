using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;

namespace FarmaDiBusiness.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISalesRepository _saleRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IStockService _stockService;

        public SaleService(ISalesRepository saleRepository, IUsersRepository usersRepository, IStockService stockService)
        {
            _saleRepository = saleRepository;
            _userRepository = usersRepository;
            _stockService = stockService;
        }

        public async Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto)
        {
            try
            {
                // 1. CORREGIDO: Validación semántica real de existencia del usuario
                var existingUser = await _userRepository.GetByIdAsync(dto.UserId);
                if (existingUser?.Data == null)
                {
                    return new ServiceResponse<SaleResponseDto>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, // CORREGIDO: Devuelve NotFound en lugar de NoData
                        Message = "El usuario que intenta registrar esta venta no existe en el sistema."
                    };
                }

                // 2. Validación perimetral de la lista de productos y stock disponible
                foreach (var detail in dto.Details)
                {
                    var existingProduct = await _stockService.GetByIdAsync(detail.ProductId); // CORREGIDO: Corrección de typo
                    if (existingProduct?.Data == null)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"El producto con Id {detail.ProductId} no existe en el inventario."
                        };
                    }

                    // Validar si hay suficientes unidades en el inventario para la venta 
                    if (existingProduct.Data.AvailableQuantity < detail.Quantity)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict, // CORREGIDO: Devuelve un Conflict (HTTP 409) legítimo
                            Message = $"El producto con Id {detail.ProductId} no tiene suficiente stock para completar la venta."
                        };
                    }
                }

                // 3. Mapeo seguro del Maestro de la venta
                var saleMaster = new Sale
                {
                    ClientName = dto.ClientName,
                    UserId = dto.UserId,
                    RegisteredDate = (DateTime.Now),
                    Discount = 0,
                    SubTotal = 0,
                    PaymentMethodId = dto.PaymentMethodId
                };

                // 4. Mapeo seguro del Detalle de la venta
                var saleDetails = dto.Details.Select(dt => new SaleDetails
                {
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity,
                    UnitPrice = dt.UnitPrice // CRITICAL BUG FIX: Faltaba inyectar el precio unitario en tu código original
                }).ToList();

                // 5. Invocación de persistencia atómica en el repositorio transaccional
                var repoResponse = await _saleRepository.InsertAsync(saleMaster, saleDetails);

                // 6. Procesamiento de la respuesta del repositorio
                if (repoResponse.OperationStatusCode == 0)
                {
                    var dataResponse = new SaleResponseDto
                    {
                        InvoiceId = repoResponse.Data!.Sale.SaleId,
                        UserId = repoResponse.Data.Sale.UserId,
                        ClientName = repoResponse.Data.Sale.ClientName,
                        SaleDate = repoResponse.Data.Sale.RegisteredDate,
                        SubTotal = repoResponse.Data.Sale.SubTotal,
                        Discount = repoResponse.Data.Sale.Discount,
                        TotalAmount = repoResponse.Data.Sale.Total,
                        PaymethMethodId = repoResponse.Data.Sale.PaymentMethodId,

                        Details = repoResponse.Data.SaleDetailsList.Select(d => new SalesDetailsResponseDto
                        {
                            ProductId = d.ProductId,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        }).ToList()
                    };

                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta registrada con éxito."
                    };
                }
                else
                {
                    return new ServiceResponse<SaleResponseDto>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = repoResponse.Message ?? "Ocurrió un error al registrar la venta en la base de datos."
                    };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Error de base de datos / servidor interno legítimo
                    Message = "Ocurrió un error inesperado al procesar la solicitud de venta."
                };
            }
        }



        public async Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id)
        {
            // 1. Validación de regla de negocio: ID debe ser válido
            if (id <= 0)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    IsSuccess = false,
                    MessageCode = MessageCodes.BadRequest,
                    Message = "El identificador de factura debe ser mayor a cero."
                };
            }

            // 2. Invocación al repositorio
            var repoResponse = await _saleRepository.GetInvoiceByIdAsync(id);

            // 3. Procesamiento de errores del repositorio
            if (repoResponse.OperationStatusCode != 0)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    IsSuccess = false,
                    MessageCode = repoResponse.OperationStatusCode == 404
                        ? MessageCodes.NotFound
                        : MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrió un error al recuperar la factura."
                };
            }

            // 4. Validación de integridad de datos
            if (repoResponse.Data?.InvoiceMaster is null)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "La factura retornada tiene datos incompletos o corruptos."
                };
            }

            // 5. MAPEO a DTO
            var invoice = repoResponse.Data.InvoiceMaster;
            var details = repoResponse.Data.InvoiceDetails ?? new List<InvoiceDetails>();

            var resultDto = new SaleResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                UserId = invoice.UserId,
                ClientName = invoice.ClientName,
                SaleDate = invoice.RegisteredDate,
                SubTotal = invoice.SubTotal,
                Discount = invoice.Discount,
                TotalAmount = invoice.Total,
               // IsPrinted = invoice.IsPrinted,
                // IMPORTANTE: Si tu SP no retorna PaymentMethodId, comenta esta línea o agrégala al SP
                // PaymethMethodId = invoice.PaymentMethodId,

                Details = details.Select(d => new SalesDetailsResponseDto
                {
                    ProductId = d.ProductId,
                    ProductTradeName = d.ProductTradeName,
                    ProductGenericName = d.ProductGenericName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                }).ToList()
            };

            return new ServiceResponse<SaleResponseDto>
            {
                Data = resultDto,
                IsSuccess = true,
                MessageCode = MessageCodes.Success,
                Message = "Factura recuperada con éxito."
            };
        }



        public async Task<ServiceResponse<PagedSaleResult>> GetSalesPagedAsync(int pageNumber, int pageSize)
        {
            // 1. Validaciones de Negocio Básicas
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10; // Evitamos cargas masivas malintencionadas

            // 2. Llamada a la capa de Datos
            var repositoryResponse = await _saleRepository.GetSalesAsync(pageNumber, pageSize);

            // 3. Mapeo y manejo del resultado del repositorio
            if (repositoryResponse.OperationStatusCode != 0)
            {
                // Si la base de datos falló, notificamos a la capa superior
                return new ServiceResponse<PagedSaleResult>
                {
                    Success = false,
                    Message = repositoryResponse.Message,
                    Data = null
                };
            }

            // 4. Retorno exitoso hacia el API Controller
            return new ServiceResponse<PagedSaleResult>
            {
                IsSuccess = true,
                Message = repositoryResponse.Message,
                Data = repositoryResponse.Data
            };
        }
    }
}
