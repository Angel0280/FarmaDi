using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;

namespace FarmaDiBusiness.Services
{
    public class ProductsService : IProductService
    {
        private readonly IProductsRepository _productRepository;
        private readonly ICategoriesService _categoryService;
        private readonly IPresentationService _presentationService;
        private readonly ISupplierService _supplierService;
        private readonly IBrandsService _brandsService;

        public ProductsService(
            IProductsRepository productRepository,
            ICategoriesService categoryService,
            IPresentationService presentationService,
            ISupplierService supplierService,
            IBrandsService brandsService)
        {
            _productRepository = productRepository;
            _categoryService = categoryService;
            _presentationService = presentationService;
            _supplierService = supplierService;
            _brandsService = brandsService;
        }

        public async Task<ServiceResponse<Products>> AddAsync(AddProductDto newProduct)
        {
            try
            {
                // Validar la existencia de las llaves foráneas relacionales
                var existsCategory = await _categoryService.GetByIdAsync(newProduct.CategoryId);
                if (existsCategory.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"No existe una categoría que coincida con el id {newProduct.CategoryId}"
                    };
                }

                var existsPresentation = await _presentationService.GetByIdAsync(newProduct.PresentationId);
                if (existsPresentation.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"No existe una presentación que coincida con el id {newProduct.PresentationId}"
                    };
                }

                var existsSupplier = await _supplierService.GetByIdAsync(newProduct.SupplierId);
                if (existsSupplier.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"No existe un proveedor que coincida con el id {newProduct.SupplierId}"
                    };
                }

                var existsBrand = await _brandsService.GetByIdAsync(newProduct.BrandId);
                if (existsBrand.Data == null)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"No existe una marca que coincida con el id {newProduct.BrandId}"
                    };
                }

                // Validar duplicidad de nombre genérico
                var existing = await _productRepository.GetByNameAsync(newProduct.GenericName);
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

                var product = new Products
                {
                    GenericName = newProduct.GenericName,
                    TradeName = newProduct.TradeName,
                    CategoryId = newProduct.CategoryId,
                    PresentationId = newProduct.PresentationId,
                    ConcentrationId = newProduct.ConcentrationId,
                    ConcentrationValue = newProduct.ConcentrationValue, // NUEVO: Inyección de la característica
                    SupplierId = newProduct.SupplierId,
                    BrandId = newProduct.BrandId,
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
            catch (Exception)
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar el producto",
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Products>>> GetAllAsync()
        {
            try
            {
                var result = await _productRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Products>>
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
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Consistencia semántica de fallos
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Products>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar los productos"
                };
            }
        }

        public async Task<ServiceResponse<Products>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _productRepository.GetByIdAsync(id);

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
            catch (Exception)
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar el producto"
                };
            }
        }

        public async Task<ServiceResponse<Products>> UpdateAsync(int id, UpdateProductDto productDto)
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

                var existingName = await _productRepository.GetByNameAsync(productDto.GenericName);
                if (existingName.OperationStatusCode == 0 && existingName.Data!.ProductId != id)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un producto con el nombre proporcionado"
                    };
                }

                var dataProduct = new Products
                {
                    GenericName = productDto.GenericName,
                    TradeName = productDto.TradeName,
                    CategoryId = productDto.CategoryId,
                    PresentationId = productDto.PresentationId,
                    ConcentrationId = productDto.ConcentrationId,
                    ConcentrationValue = productDto.ConcentrationValue, // NUEVO: Inyección de la característica
                    SupplierId = productDto.SupplierId,
                    BrandId = productDto.BrandId,
                    IsActive = productDto.IsActive,
                };

                var result = await _productRepository.UpdateAsync(id, dataProduct);

                return new ServiceResponse<Products>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Producto actualizado correctamente"
                };
            }
            catch (Exception)
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
            try
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

                var messageCode = MessageCodes.Success;
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
            catch (Exception)
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar por nombre"
                };
            }
        }

        public async Task<ServiceResponse<Products>> SetStateAsync(int id, bool state)
        {
            try
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
            catch (Exception)
            {
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al modificar el estado del producto"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>> GetProductsPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _productRepository.GetProductsPagedAsync(page, limit);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                var messageCode = MessageCodes.Success;
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
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Products> Items, int TotalCount)>
                {
                    Data = (null, 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar los productos"
                };
            }
        }
    }
}