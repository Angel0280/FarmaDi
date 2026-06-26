using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiBusiness.Services
{
    public class BrandsService : IBrandsService
    {
        private readonly IBrandsRepository _brandRepository;

        public BrandsService(IBrandsRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<ServiceResponse<Brands>> AddAsync(AddBrandDto newBrand)
        {
            try
            {
                // Validar si existe una marca con el mismo nombre
                var existing = await _brandRepository.GetByNameAsync(newBrand.BrandName);

                if (existing?.Data != null && existing.Data.BrandId != 0 && !existing.Data.BrandName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Brands>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                var brand = new Brands
                {
                    BrandName = newBrand.BrandName,
                    Description = newBrand.BrandDescription,
                };

                var result = await _brandRepository.AddAsync(brand);

                return new ServiceResponse<Brands>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Marca registrada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Brands>>> GetAllAsync()
        {
            try
            {
                var result = await _brandRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Brands>>
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
                        return new ServiceResponse<IEnumerable<Brands>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Brands>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Antes NoData
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Brands>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar los datos"
                };
            }
        }

        public async Task<ServiceResponse<Brands>> GetByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Movido dentro del try-catch de forma segura
                var result = await _brandRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Brands>
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
                        return new ServiceResponse<Brands>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La marca no existe"
                        };
                    default:
                        return new ServiceResponse<Brands>
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
                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<Brands>> UpdateAsync(int id, UpdateBrandDto brandDto)
        {
            try
            {
                // Validar existencia por ID de manera segura
                var existingId = await _brandRepository.GetByIdAsync(id);
                if (existingId?.Data == null || (existingId.Data.BrandId == 0 && existingId.Data.BrandName.IsNullOrEmpty()))
                {
                    return new ServiceResponse<Brands>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe una marca asociada al Id proporcionado"
                    };
                }

                // Validar que el nuevo nombre no genere conflicto con otra marca distinta
                var existingName = await _brandRepository.GetByNameAsync(brandDto.BrandName);
                if (existingName?.Data != null && existingName.Data.BrandName != null && existingName.Data.BrandId != id)
                {
                    return new ServiceResponse<Brands>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una marca con el nombre proporcionado"
                    };
                }

                var dataBrand = new Brands
                {
                    BrandName = brandDto.BrandName,
                    Description = brandDto.BrandDescription,
                    IsActive = brandDto.IsActive,
                };

                var result = await _brandRepository.UpdateAsync(id, dataBrand);

                return new ServiceResponse<Brands>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Marca actualizada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar la marca"
                };
            }
        }

        public async Task<ServiceResponse<Brands>> GetByNameAsync(string name)
        {
            try
            {
                var result = await _brandRepository.GetByNameAsync(name);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Brands>
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
                        message = "No se encontró una marca que corresponda al nombre proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener la marca.";
                        break;
                }

                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la búsqueda por nombre"
                };
            }
        }

        public async Task<ServiceResponse<Brands>> SetStateAsync(int id, bool state)
        {
            try
            {
                var response = new ServiceResponse<Brands>();

                // CORREGIDO: Validación de existencia real alineada al comportamiento del Repositorio
                var existing = await _brandRepository.GetByIdAsync(id);
                if (existing?.Data == null || existing.Data.BrandId == 0)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.ErrorValidation;
                    response.Message = "La marca no existe";
                    return response;
                }

                var repoResponse = await _brandRepository.SetStateAsync(id, state);

                if (repoResponse?.Data == null)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.NotFound;
                    response.Message = "No se pudo encontrar una marca que coincida con el id proporcionado";
                    return response;
                }

                response.Data = repoResponse.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "Marca activada" : "Marca desactivada";

                return response;
            }
            catch (Exception)
            {
                return new ServiceResponse<Brands>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al modificar el estado de la marca"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Brands> Items, int TotalCount)>> GetBrandsPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _brandRepository.GetBrandsPagedAsync(page, limit);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Brands> Items, int TotalCount)>
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
                        message = "No se encontraron marcas para la página solicitada";
                        break;
                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener las marcas.";
                        break;
                }

                return new ServiceResponse<(IEnumerable<Brands> Items, int TotalCount)>
                {
                    Data = (new List<Brands>(), 0),
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Brands> Items, int TotalCount)>
                {
                    Data = (new List<Brands>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar los registros"
                };
            }
        }
    }
}