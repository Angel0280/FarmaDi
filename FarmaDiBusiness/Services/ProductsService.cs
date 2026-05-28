using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens; // <-- Esta dependencia parece no usarse
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class ProductsService : IProductService
    {
        // ... (Constructor sin cambios, está correcto) ...
        private readonly IProductsRepository _productRepository;
        private readonly ICategoriesService _CategoryService;
        private readonly IPresentationService _PresentationService;
        //private readonly IConcentrationService _ConcentrationService;
        private readonly ISupplierService _SupplierService;
        private readonly IBrandsService _BrandsService;
        public ProductsService(IProductsRepository productRepository, ICategoriesService categoryService, IPresentationService presentationService, ISupplierService supplierService, IBrandsService brandsService)
        {
            _productRepository = productRepository;
            _CategoryService = categoryService;
            _PresentationService = presentationService;
            _SupplierService = supplierService;
            _BrandsService = brandsService;
        }


        public async Task<ServiceResponse<Products>> AddAsync(AddProductDto newproduct)
        {
            try
            {
                
                var existCategory = await _CategoryService.GetByIdAsync(newproduct.CategoryId);
                if (existCategory.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"no existe una categoria que coincida con el id {newproduct.CategoryId}"
                    };
                }
                var existPresentation = await _PresentationService.GetByIdAsync(newproduct.PresentationId);
                if (existPresentation.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"no existe una presentación que coincida con el id {newproduct.PresentationId}"
                    };
                }
                
                var existSupplier = await _SupplierService.GetByIdAsync(newproduct.SupplierId);
                if (existSupplier.Data == null)
                {
                    return new ServiceResponse<Products> { /* ... */ };
                }
                var existBrand = await _BrandsService.GetByIdAsync(newproduct.BrandId);
                if (existBrand.Data == null)
                {
                    return new ServiceResponse<Products> { /* ... */ };
                }

                
                var existing = await _productRepository.GetByNameAsync(newproduct.GenericName);

                if (existing.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }


                var product = new Products()
                {
                    GenericName = newproduct.GenericName,
                    TradeName = newproduct.TradeName,
                    CategoryId = newproduct.CategoryId,
                    PresentationId = newproduct.PresentationId,
                    ConcentrationId = newproduct.ConcentrationId,
                    SupplierId = newproduct.SupplierId,
                    BrandId = newproduct.BrandId,
                };

                var result = await _productRepository.AddAsync(product);

                return new ServiceResponse<Products>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Producto registrado correctamente"
                };
            }
            catch (Exception ex) 
            {
                
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Products>>> GetAllAsync()
        {
            var result = await _productRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Products>>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }
            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<IEnumerable<Products>>
                    {
                        Data = result.Data,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<Products>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Products>> GetByIdAsync(int id)
        {
            var result = await _productRepository.GetByIdAsync(id);
            try
            {
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Products>
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
                        return new ServiceResponse<Products>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El producto no existe"
                        };
                    default:
                        return new ServiceResponse<Products>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Error inesperado"
                        };
                }
            }
            catch (Exception ex) 
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<Products>> UpdateAsync(int id, UpdateProductDto Products)
        {
            try
            {
                var existingId = await _productRepository.GetByIdAsync(id);
                if (existingId.OperationStatusCode != 0) 
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, 
                        Message = "No existe un producto asociado al Id proporcionado"
                    };
                }

                var existingName = await _productRepository.GetByNameAsync(Products.GenericName);
                if (existingName.OperationStatusCode == 0 && existingName.Data.ProductId != id)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe un producto con el nombre proporcionado"
                    };
                }

                var dataproduct = new Products()
                {
                    GenericName = Products.GenericName,
                    TradeName = Products.TradeName,
                    CategoryId = Products.CategoryId,
                    PresentationId = Products.PresentationId,
                    ConcentrationId = Products.ConcentrationId,
                    SupplierId = Products.SupplierId,
                    BrandId = Products.BrandId,
                    IsActive = Products.IsActive,
                };

                var result = await _productRepository.UpdateAsync(id, dataproduct);

                return new ServiceResponse<Products>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Producto actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar el producto"
                };
            }
        }

        public async Task<ServiceResponse<Products>> GetByNameAsync(string name)
        {
            var result = await _productRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Products>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 50009:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontró un producto que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el producto.";
                    break;
            }

            return new ServiceResponse<Products>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<Products>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Products>();

            
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing.OperationStatusCode != 0)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound; 
                response.Message = "El producto no existe";
                return response;
            }

           
            var repoResponse = await _productRepository.SetStateAsync(id, state);

            
            if (repoResponse.OperationStatusCode != 0)
            {
                response.Data = null;
                response.IsSuccess = false;
              
                response.MessageCode = (repoResponse.OperationStatusCode == 50009) ? MessageCodes.NotFound : MessageCodes.ErrorDataBase;
                response.Message = repoResponse.Message ?? "No se pudo actualizar el estado del producto";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Producto activado" : "Producto desactivado";

            return response;
        }

        public async Task<ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>> GetProductsPagedAsync(int page, int limit)
        {
            var result = await _productRepository.GetProductsPagedAsync(page, limit);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }
            var messageCode = new MessageCodes();
            var message = string.Empty;
            switch (result.OperationStatusCode)
            {
                case 50009:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontraron productos para la página solicitada";
                    break;
                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener los productos paginados.";
                    break;
            }
            return new ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>
            {
                Data = (null, 0),
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };

        }
    }
}