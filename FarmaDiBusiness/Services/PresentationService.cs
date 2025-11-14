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
    public class PresentationService : IPresentationService
    {
        private readonly IPresentationRepository _presentationRepository;

        public PresentationService(IPresentationRepository presentationRepository)
        {
            _presentationRepository = presentationRepository;
        }




        public async Task<ServiceResponse<Presentations>> AddAsync(AddPresentationDto newpresentation)
        {

            try
            {
              
                var existing = await _presentationRepository.GetByNameAsync(newpresentation.Description);

                if (existing.Data!.Id != 0 && !existing.Data.Description.IsNullOrEmpty())
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }


                var presentation = new Presentations()
                {
                    Description = newpresentation.Description,
                    Quantity = newpresentation.Quantity,
                    UnitMeasure = newpresentation.UnitMeasure,
                    IsActive = true,


                };

                var result = await _presentationRepository.AddAsync(presentation);

                return new ServiceResponse<Presentations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Presentación registrada correctamente"
                };



            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }


        public async Task<ServiceResponse<IEnumerable<Presentations>>> GetAllAsync()
        {
            var result = await _presentationRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Presentations>>()
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
                    return new ServiceResponse<IEnumerable<Presentations>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<Presentations>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };

            }

        }


        public async Task<ServiceResponse<Presentations>> GetByIdAsync(int id)
        {
            var result = await _presentationRepository.GetByIdAsync(id);
            try
            {
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Presentations>
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
                        return new ServiceResponse<Presentations>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La presentación no existe"
                        };
                    default:
                        return new ServiceResponse<Presentations>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Ocurrió un error inesperado al obtener la presentación"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Ocurrió un error inesperado"

                };
            }
        }


        public async Task<ServiceResponse<Presentations>> UpdateAsync(int id, UpdatePresentationDto presentacion)
        {

            try
            {

                var existingId = await _presentationRepository.GetByIdAsync(id);
                if (existingId.Data!.Id == 0 && existingId.Data.Description.IsNullOrEmpty())
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe una presentación asociada al Id proporcionado"

                    };


                }

                //validar que el nombre enviado para la marca no coincida con un  nombre existente
                var existingName = await _presentationRepository.GetByNameAsync(presentacion.Description);
                if (existingName.Data!.Description != null && existingName.Data.Id != id)
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe una presentación con el nombre proporcionado"
                    };
                }

                var data = new Presentations()
                {
                    Description = presentacion.Description,
                    Quantity = presentacion.Quantity,
                    UnitMeasure = presentacion.UnitMeasure,
                    IsActive = presentacion.IsActive,


                };

                var result = await _presentationRepository.UpdateAsync(id, data);

                return new ServiceResponse<Presentations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Marca actualizada correctamente"
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar la presentación"
                };
            }
        }


        public async Task<ServiceResponse<Presentations>> GetByNameAsync(string name)
        {
            var result = await _presentationRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Presentations>
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
                case 5038:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontró una presentación que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la marca.";
                    break;
            }

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Presentations>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }



        public async Task<ServiceResponse<Presentations>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Presentations>();

            // Validar que la presentación exista
            var existing = await _presentationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "La presentación no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _presentationRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado de la presentación";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "presentación activada" : "presentación desactivada";

            return response;
        }
    }
}