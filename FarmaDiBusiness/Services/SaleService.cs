using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISalesRepository _saleRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IStockService _stockService;
        public SaleService
            (ISalesRepository saleRepository,
            IUsersRepository usersRepository,
            IStockService stockService)
        {
            _saleRepository = saleRepository;
            _userRepository = usersRepository;
            _stockService = stockService;
        }
        public async Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto)
        {
            try
            {
                //Validar que existe el usuario
                var existingUser = await _userRepository.GetByIdAsync(dto.UserId);
                if (existingUser == null)
                {
                    return new ServiceResponse<SaleResponseDto>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "El usuario que quiere registra esta venta no esta registrado."
                    };
                }

                //Validar que cada producto del detalle exista en el inventario
                foreach (var detail in dto.Details)
                {
                    var existenProduct = await _stockService.GetByIdAsync(detail.ProductId);
                    if (existenProduct.Data == null)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = $"El producto con Id {detail.ProductId} no existe en el inventario."
                        };
                    }
                    //Validar si hay suficientes unidades en el inventario para venta 
                    if (existenProduct.Data!.AvailableQuantity < detail.Quantity)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = $"El producto con Id {detail.ProductId} no tiene suficiente stock para completar la venta."
                        };
                    }
                }
                //Mapeo del Maestro de la venta
                var saleMaster = new Sale { 
                    ClientName = dto.ClientName,
                    UserId = dto.UserId,
                    RegisteredDate = DateOnly.FromDateTime(DateTime.Now),
                    SubTotal = 0,
                    Discount = 0,


                };
                //Mapeo del Detalle de la venta
                var saleDetails =dto.Details.Select(dt => new SaleDetails { 
                
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity

                }).ToList();
                


                //Invovamos al metodo de insertar la venta del repositorio
                var repoResponse = await _saleRepository.InsertAsync(saleMaster, saleDetails);

                //Verificamos la respuesta del repositorio
                if (repoResponse.OperationStatusCode == 0)
                {
                    //Mapeo de la respuesa al Dto de respuesta
                    var dataResponse = new SaleResponseDto();

                    dataResponse.InvoiceId = repoResponse.Data!.Sale.SaleId;
                    dataResponse.UserId = repoResponse.Data!.Sale.UserId;
                    dataResponse.ClientName = repoResponse.Data!.Sale.ClientName;
                    dataResponse.SaleDate = repoResponse.Data!.Sale.RegisteredDate;
                    dataResponse.SubTotal = repoResponse.Data!.Sale.SubTotal;
                    dataResponse.Discount = repoResponse.Data!.Sale.Discount;
                    dataResponse.TotalAmount = repoResponse.Data!.Sale.Total;
                    dataResponse.PaymethMethodId = repoResponse.Data!.Sale.PaymentMethodId;
                    dataResponse.Details = repoResponse.Data!.SaleDetailsList.Select(d => new SalesDetailsResponseDto
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        TotalPrice = d.SubTotal
                    }).ToList();

                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta registrada con exito."
                    };
                }
                else
                {
                    return new ServiceResponse<SaleResponseDto>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrio un error al registrar la venta."
                    };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorValidation,
                    Message = "Ocurrio un error inesperado."
                };
            }
            
        }
    }
}
