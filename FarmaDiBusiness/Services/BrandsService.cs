using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class BrandsService : IBrandsService
    {

        // Implementacion del metodo GetAllAsync para obtener todas las marcas
        private readonly IBrandsRepository _brandRepository;
        public BrandsService(IBrandsRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }


        public async Task<ServiceResponse<Brands>> AddAsync(AddBrandDto newbrand)
        {

            try
            {
                 //validar si existe registro (una marca) con nombre similar al que se desea crear
                  var existing = await _brandRepository.GetByNameAsync(newbrand.BrandName);

                if (existing.Data!.BrandId != 0 && !existing.Data.BrandName.IsNullOrEmpty())
                 {
                     return new ServiceResponse<Brands>
                     {
                         Data = null,
                         IsSuccess = false, ///.//
                         MessageCode = MessageCodes.Conflict ,
                         Message = "Existe un registro con el nombre proporcionado"

                     };

                 }
                

                var brand = new Brands()
                {
                    BrandName = newbrand.BrandName,
                    Description = newbrand.BrandDescription,

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
                    Message = "Ocurrió un error inesperado",
                };
            }
        }


        public async Task<ServiceResponse<IEnumerable<Brands>>> GetAllAsync()
        {
            var result = await _brandRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Brands>>()
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
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };

            }

        }

        public async Task<ServiceResponse<Brands>> GetByIdAsync(int id)
        {
            var result = await _brandRepository.GetByIdAsync(id);
            try
            {
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
                    case 50009: // Ejemplo: código para no encontrado
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
                    Message = result.Message ?? "Ocurrió un error inesperado"

                };
            }
        }

        public async Task<ServiceResponse<Brands>> UpdateAsync(int id, UpdateBrandDto brands)
        {

            try
            {

                var existingId = await _brandRepository.GetByIdAsync(id);
                if (existingId.Data!.BrandId == 0 && existingId.Data.BrandName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Brands>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe una marca asociada al Id proporcionado"

                    };


                }

                //validar que el nombre enviado para la marca no coincida con un  nombre existente
                var existingName = await _brandRepository.GetByNameAsync(brands.BrandName);
                if (existingName.Data!.BrandName != null && existingName.Data.BrandId != id)
                {
                    return new ServiceResponse<Brands>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe una marca con el nombre proporcionado"
                    };
                }

                var dataBrand = new Brands()
                {
                    BrandName = brands.BrandName,
                    Description = brands.BrandDescription,
                    IsActive = brands.IsActive,

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

            var messageCode = new MessageCodes();
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

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Brands>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }




        public async Task<ServiceResponse<Brands>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Brands>();

            // Validar que la marca exista
            var existing = await _brandRepository.GetByIdAsync(id);
            if (existing == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "La marca no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _brandRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound;
                response.Message = "No se pudo encontrar una marca que coincida con el id proporcionado";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Marca activada" : "Marca desactivada";

            return response;
        }

    }




}

