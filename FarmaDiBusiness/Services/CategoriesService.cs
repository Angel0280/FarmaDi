using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiBusiness.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly ICategoriesRepository _categoryRepository;

        public CategoriesService(ICategoriesRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ServiceResponse<Categories>> AddAsync(AddCategoryDto newCategory)
        {
            try
            {
                // Validar si existe una categoría con el mismo nombre
                var existing = await _categoryRepository.GetByNameAsync(newCategory.CategoryName);

                if (existing?.Data != null && existing.Data.CategoryId != 0 && !existing.Data.CategoryName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                var category = new Categories
                {
                    CategoryName = newCategory.CategoryName,
                    CategoryDescription = newCategory.CategoryDescription,
                };

                var result = await _categoryRepository.AddAsync(category);

                return new ServiceResponse<Categories>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Categoría registrada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Categories>>> GetAllAsync()
        {
            try
            {
                var result = await _categoryRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Categories>>
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
                        return new ServiceResponse<IEnumerable<Categories>>
                        {
                            Data = result.Data,
                            IsSuccess = false, // CORREGIDO: Consistencia de estado fallido
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Categories>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Mapeo correcto de error de base de datos
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Categories>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar las categorías"
                };
            }
        }

        public async Task<ServiceResponse<Categories>> GetByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Ejecución resguardada dentro del bloque try
                var result = await _categoryRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Categories>
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
                        return new ServiceResponse<Categories>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La categoría no existe"
                        };
                    default:
                        return new ServiceResponse<Categories>
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
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<Categories>> UpdateAsync(int id, UpdateCategoryDto categoryDto)
        {
            try
            {
                // Validar existencia de la categoría por ID de forma segura
                var existingId = await _categoryRepository.GetByIdAsync(id);
                if (existingId?.Data == null || (existingId.Data.CategoryId == 0 && existingId.Data.CategoryName.IsNullOrEmpty()))
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe una categoría asociada al Id proporcionado"
                    };
                }

                // Validar que el nuevo nombre no cause conflicto con un registro de ID diferente
                var existingName = await _categoryRepository.GetByNameAsync(categoryDto.Name);
                if (existingName?.Data != null && existingName.Data.CategoryName != null && existingName.Data.CategoryId != id)
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una categoría con el nombre proporcionado"
                    };
                }

                var categoryEntity = new Categories
                {
                    CategoryName = categoryDto.Name,
                    CategoryDescription = categoryDto.Description,
                    IsActive = categoryDto.IsActive,
                };

                var result = await _categoryRepository.UpdateAsync(id, categoryEntity);

                return new ServiceResponse<Categories>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Categoría actualizada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar la categoría"
                };
            }
        }

        public async Task<ServiceResponse<Categories>> GetByNameAsync(string name)
        {
            try
            {
                var result = await _categoryRepository.GetByNameAsync(name);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Categories>
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
                        message = "No se encontró una categoría que corresponda al nombre proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener la categoría.";
                        break;
                }

                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la consulta por nombre"
                };
            }
        }

        public async Task<ServiceResponse<Categories>> SetStateAsync(int id, bool state)
        {
            try
            {
                var response = new ServiceResponse<Categories>();

                // CORREGIDO: Evaluación de existencia alineada al comportamiento real del Repositorio
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory?.Data == null || existingCategory.Data.CategoryId == 0)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.NotFound;
                    response.Message = "La categoría no existe";
                    return response;
                }

                var repoResponse = await _categoryRepository.SetStateAsync(id, state);

                if (repoResponse?.Data == null)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.NotFound;
                    response.Message = "No se pudo encontrar una categoría relacionada al id brindado";
                    return response;
                }

                response.Data = repoResponse.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "Categoría activada" : "Categoría desactivada";

                return response;
            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al modificar el estado de la categoría"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Categories> Items, int TotalCount)>> GetCategoriesPaged(int page, int limit)
        {
            try
            {
                var result = await _categoryRepository.GetCategoriesPagedAsync(page, limit);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Categories> Items, int TotalCount)>
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
                        // CORREGIDO: Mensaje adecuado al contexto del módulo
                        message = "No se encontraron categorías para la página solicitada";
                        break;
                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        // CORREGIDO: Mensaje adecuado al contexto del módulo
                        message = "Error en la base de datos al obtener las categorías.";
                        break;
                }

                return new ServiceResponse<(IEnumerable<Categories> Items, int TotalCount)>
                {
                    Data = (new List<Categories>(), 0),
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Categories> Items, int TotalCount)>
                {
                    Data = (new List<Categories>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar los registros"
                };
            }
        }
    }
}