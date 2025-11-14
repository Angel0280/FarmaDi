using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Roles;
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
    public class RolesService : IRolService
    {
        private readonly IRolesRepository _rolRepository;
        public RolesService(IRolesRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<ServiceResponse<Roles>> AddRolAsync(AddRolDto newRol)
        {
            try
            {
                //validar si existe registro (un rol) con nombre similar al que se desea crear
                var existing = await _rolRepository.GetByNameAsync(newRol.RolName);

                if (existing.Data!.Id != 0 && !existing.Data.RolName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un rol con el nombre proporcionado"

                    };

                }
                var rol = new Roles()
                {
                 // le pasamos al repo unicamente el capo requerido que viene del dto,... vamos al controlador.........  (viniendo del repositorio) en realidad es la reves
                    RolName = newRol.RolName,
                    

                };

                var result = await _rolRepository.AddAsync(rol);

                return new ServiceResponse<Roles>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol registrado correctamente"
                };



            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Roles>>> GetAllRolsAsync()
        {
            var result = await _rolRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Roles>>()
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
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };

            }
        }

        public async Task<ServiceResponse<Roles>> GetRolByIdAsync(int id)
        {
            var result = await _rolRepository.GetByIdAsync(id);
            try
            {
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operacion exitosa"
                    };
                }
                switch (result.OperationStatusCode)
                {
                    case 50009: // Ejemplo: código para no encontrado
                        return new ServiceResponse<Roles>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El rol asociado a este Id no existe"
                        };
                    default:
                        return new ServiceResponse<Roles>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = "Ocurrió un error inesperado al obtener el rol"
                        };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ServiceResponse<Roles>> UpdateRolAsync(int id, UpdateRolDto rol)
        {
            try
            {

                var existingId = await _rolRepository.GetByIdAsync(id);
                if (existingId.Data!.Id == 0 && existingId.Data.RolName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un rol relacionado al Id proporcionado"
                    };
                }

                //validar que el nombre enviado para la marca no coincida con un  nombre existente
                var existingName = await _rolRepository.GetByNameAsync(rol.RolName);
                if (existingName.Data!.RolName != null && existingName.Data.Id != id)
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un rol con el nombre proporcionado"
                    };
                }

                var dataRoles = new Roles()
                {
                    RolName = rol.RolName,
                    IsActive = rol.IsActive,

                };

                var result = await _rolRepository.UpdateAsync(id, dataRoles);

                return new ServiceResponse<Roles>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol actualizado correctamente"
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar el rol"
                };
            }
        }

        public async Task<ServiceResponse<Roles>> GetRolByNameAsync(string name)
        {
            var result = await _rolRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Roles>
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
                    message = "No se encontró un rol que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el rol.";
                    break;
            }

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Roles>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };

        }

        public async Task<ServiceResponse<Roles>> SetRolStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Roles>();

            // Validar que el rol exista
            var existing = await _rolRepository.GetByIdAsync(id);
            if (existing == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El rol no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _rolRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del rol";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Rol activado" : "Rol desactivado";

            return response;
        }
    }
    
}
