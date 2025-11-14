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
                // --- (Validación de FKs (Categoría, Presentación, Proveedor, Marca) sin cambios, está correcta) ---
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
                // ... (Omitido para brevedad - las otras 2 validaciones de FK están bien) ...
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

                // --- Validación de duplicados ---

                // <-- CORRECCIÓN: Error Crítico de Nulidad
                // El código original (existing.Data!.ProductId != 0) causaría un NullReferenceException
                // si GetByNameAsync devuelve un 'Data = null' (porque no encontró el producto).
                // La lógica correcta es: si la operación fue exitosa (StatusCode 0), significa que
                // SÍ encontró un producto, y por lo tanto es un duplicado.
                var existing = await _productRepository.GetByNameAsync(newproduct.GenericName);

                if (existing.OperationStatusCode == 0) // 0 = Éxito (Significa que SÍ lo encontró)
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
            catch (Exception ex) // <-- CORRECCIÓN: Buena práctica capturar 'ex' para logging
            {
                // TODO: Registrar el error (ex.Message) en un sistema de logs
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }

        // <-- (El método GetAllAsync() está bien implementado, sin cambios) -->
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
            // ... (El switch está bien) ...
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
                    case 50009: // Ejemplo: código para no encontrado
                        return new ServiceResponse<Products>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            // <-- CORRECCIÓN: Typo "no existe no existe"
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
            catch (Exception ex) // <-- CORRECCIÓN: Capturar 'ex'
            {
                // TODO: Registrar el error (ex.Message) en un sistema de logs
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    // <-- CORRECCIÓN: 'result' no está en el ámbito (scope) del catch.
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<Products>> UpdateAsync(int id, UpdateProductDto Products)
        {
            try
            {
                // --- (Validación de FKs (Categoría, Presentación, Proveedor, Marca) sin cambios, está correcta) ---
                // ... (Omitido para brevedad) ...

                // --- Validación de existencia ---

                // <-- CORRECCIÓN: Error Crítico de Nulidad (Mismo caso que en AddAsync)
                // La lógica debe ser: si el 'OperationStatusCode' NO es 0, significa que no se encontró.
                var existingId = await _productRepository.GetByIdAsync(id);
                if (existingId.OperationStatusCode != 0) // 0 = Éxito (Lo encontró)
                {
                    return new ServiceResponse<Products>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, // <-- Más específico que ErrorValidation
                        Message = "No existe un producto asociado al Id proporcionado"
                    };
                }


                // --- Validación de duplicados (al actualizar) ---

                // <-- CORRECCIÓN: Error Crítico de Nulidad (Mismo caso que en AddAsync)
                // La lógica es:
                // 1. Buscar si existe otro producto con el nuevo nombre.
                // 2. Si SÍ existe (StatusCode 0) Y el ID de ese producto NO es el ID que estamos actualizando,
                //    entonces es un conflicto.
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
                    // <-- CORRECCIÓN: Typo (Decía "Marca actualizada")
                    Message = "Producto actualizado correctamente"
                };
            }
            catch (Exception ex) // <-- CORRECCIÓN: Capturar 'ex'
            {
                // TODO: Registrar el error (ex.Message) en un sistema de logs
                return new ServiceResponse<Products>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar el producto"
                };
            }
        }

        // <-- (El método GetByNameAsync() está bien implementado, sin cambios) -->
        public async Task<ServiceResponse<Products>> GetByNameAsync(string name)
        {
            // ... (Este método está bien) ...
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

            // <-- CORRECCIÓN: Error Lógico. 'existing' es un RepositoryResponse, no la entidad.
            // No se debe comparar 'existing == null', sino su 'OperationStatusCode' o 'Data'.
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing.OperationStatusCode != 0) // 0 = Éxito (Lo encontró)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound; // <-- Más específico
                response.Message = "El producto no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _productRepository.SetStateAsync(id, state);

            // <-- CORRECCIÓN: Es mejor comprobar el StatusCode que 'Data == null'
            if (repoResponse.OperationStatusCode != 0)
            {
                response.Data = null;
                response.IsSuccess = false;
                // Usar el código de error apropiado (NotFound si es 50009, sino ErrorDB)
                response.MessageCode = (repoResponse.OperationStatusCode == 50009) ? MessageCodes.NotFound : MessageCodes.ErrorDataBase;
                response.Message = repoResponse.Message ?? "No se pudo actualizar el estado del producto";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Producto activado" : "Producto desactivado";

            return response;
        }
    }
}