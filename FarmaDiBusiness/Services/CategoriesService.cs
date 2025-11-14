using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class CategoriesService :ICategoriesService
    {
        // Implementacion del metodo GetAllAsync para obtener todas las categorias
        private readonly ICategoriesRepository _categoryRepository;
        public CategoriesService(ICategoriesRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        public async Task<ServiceResponse<Categories>> AddAsync(AddCategoryDto newcategory)
        {

            try
            {
                //validar si existe registro (categoria) con nombre similar al que se desea crear
                var existing = await _categoryRepository.GetByNameAsync(newcategory.CategoryName);

                if (existing.Data!.CategoryId != 0 && !existing.Data.CategoryName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }


                var categories = new Categories()
                {
                    CategoryName = newcategory.CategoryName,
                    CategoryDescription = newcategory.CategoryDescription,

                };

                var result = await _categoryRepository.AddAsync(categories);

                return new ServiceResponse<Categories>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Categoria registrada correctamente"
                };



            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }


        public async Task<ServiceResponse<IEnumerable<Categories>>> GetAllAsync()
        {
            var result = await _categoryRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Categories>>()
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
                    return new ServiceResponse<IEnumerable<Categories>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<Categories>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrio un error inesperado"
                    };

            }

        }

        public async Task<ServiceResponse<Categories>> GetByIdAsync(int id)
        {
            var result = await _categoryRepository.GetByIdAsync(id);
            try
            {
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
                    case 50009: // Ejemplo: código para no encontrado
                        return new ServiceResponse<Categories>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La categoria no existe"
                        };
                    default:
                        return new ServiceResponse<Categories>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Ocurrió un error inesperado al obtener la categoria"
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
                    Message = result.Message ?? "Ocurrió un error inesperado"

                };
            }
        }

        public async Task<ServiceResponse<Categories>> UpdateAsync(int id, UpdateCategoryDto category)
        {

            try
            {

                var existingId = await _categoryRepository.GetByIdAsync(id);
                if (existingId.Data!.CategoryId == 0 && existingId.Data.CategoryName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe una categoria asociada al Id proporcionado"

                    };


                }

                //validar que el nombre enviado para la marca no coincida con un  nombre existente
                var existingName = await _categoryRepository.GetByNameAsync(category.Name);
                if (existingName.Data!.CategoryName != null && existingName.Data.CategoryId != id)
                {
                    return new ServiceResponse<Categories>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe una categoria con el nombre proporcionado"
                    };
                }

                var data = new Categories()
                {
                    CategoryName = category.Name,
                    CategoryDescription = category.Description,
                    IsActive = category.IsActive,

                };

                var result = await _categoryRepository.UpdateAsync(id, data);

                return new ServiceResponse<Categories>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Categoria actualizada correctamente"
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<Categories>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar la categoria"
                };
            }
        }


        public async Task<ServiceResponse<Categories>> GetByNameAsync(string name)
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

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 50009:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontró una categoria que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la categoria.";
                    break;
            }

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Categories>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }




        public async Task<ServiceResponse<Categories>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Categories>();

            // Validar que la marca exista
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound;
                response.Message = "La categoria no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _categoryRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound;
                response.Message = "No se pudo encontrar una categoria relacionada al id brindado";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Categoria activada" : "Categoria desactivada";

            return response;
        }

    }

}

