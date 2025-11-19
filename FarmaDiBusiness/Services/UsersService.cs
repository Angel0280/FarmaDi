using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _userRepository;
        private readonly IRolesRepository _rolesRepository;

        public UsersService(IUsersRepository userRepository, IRolesRepository rolesRepository)
        {
            _userRepository = userRepository;
            _rolesRepository = rolesRepository;
        }
        public async Task<ServiceResponse<RolesUers>> RegisterUserWithRolesAsync(RegisterUserRolesDto userDto)
        {
            try
            {

                /*var existingNameUser = await _userRepository.GetByUserNameAsync(userDto.UserName);
                if (existingNameUser.Data != null)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El nombre de usuario ya está en uso."
                    };
                }
                var existingEmailUser = await _userRepository.GetByEmailAsync(userDto.Mail);
                if (existingEmailUser.Data != null)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El correo electrónico ya está en uso."
                    };
                }*/
                foreach (var roleDto in userDto.RolesIds)
                {
                var existingRol = await _rolesRepository.GetByIdAsync(roleDto.IdRoles);
                    if (existingRol.Data == null)
                    {
                        return new ServiceResponse<RolesUers>
                        {
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"El rol con ID {roleDto.IdRoles} no existe."
                        };
                    }
                }

                var newUser = new Users
                {
                    UserName = userDto.UserName,
                    UserLastName = userDto.UserLastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    Mail = userDto.Mail,
                    UserPhone = userDto.UserPhone
                };

                var rolesList = userDto.RolesIds.Select(r => new Roles
                {
                    Id = r.IdRoles
                }).ToList();

                var result = await _userRepository.RegisterUserWithRolesAsync(newUser, rolesList);

                if  (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Usuario registrado exitosamente con roles."
                    };
                }
                else
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Error al registrar el usuario con roles."
                    };
                }



            }
            catch (Exception)
            {
                return new ServiceResponse<RolesUers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado."
                };
            }

        }
        
        public async Task<ServiceResponse<Users>> GetUSerByNameAsync(string name)
        {
            var result = await _userRepository.GetByUserNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Users>
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
                    message = "No se encontró un usuario que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el usuario.";
                    break;
            }

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Users>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        
    }
}
