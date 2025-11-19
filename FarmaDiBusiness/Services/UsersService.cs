using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.RolsDto;
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

                var existingNameUser = await _userRepository.GetByUserNameAsync(userDto.UserName);
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
                }
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

        public async Task<ServiceResponse<IEnumerable<GetUsers>>> GetAllAsync()
        {
            try
            {
                var repositoryResult = await _userRepository.GetAllAsync();

                // 2. Verificamos si hubo error en la base de datos
                if (repositoryResult.OperationStatusCode != 0)
                {
                    return new ServiceResponse<IEnumerable<GetUsers>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Error al consultar la base de datos: "
                    };
                }

                var usersDto = repositoryResult.Data.Select(user => new GetUsers
                {
                    Id = user.UserId,
                    UserName = user.UserName,
                    UserLastName = user.UserLastName,
                    Mail = user.Mail,
                    UserPhone = user.UserPhone,
                    IsActive = user.IsActive,

                    // Mapeo anidado: Por cada usuario, transformamos sus roles
                    Roles = user.Roles.Select(rol => new RolesResponseDto
                    {
                        RolId = rol.Id,
                        RolName = rol.RolName
                    }).ToList()

                }).ToList();

                // 4. Retornamos la respuesta exitosa
                return new ServiceResponse<IEnumerable<GetUsers>>
                {
                    Data = usersDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Listado obtenido correctamente."
                };

            }
            catch (Exception ex)
            {
                // Loguear el error real aquí (ex)
                return new ServiceResponse<IEnumerable<GetUsers>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener los usuarios."
                };
            }

        }

        public async Task<ServiceResponse<GetUsers>> GetByIdAsync(int id)
        {
            try
            {
                var repositoryResult = await _userRepository.GetByIdAsync(id);
                if (repositoryResult.OperationStatusCode != 0)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "Usuario no encontrado."
                    };
                }
                var user = repositoryResult.Data;
                var userDto = new GetUsers
                {
                    Id = user.UserId,
                    UserName = user.UserName,
                    UserLastName = user.UserLastName,
                    Mail = user.Mail,
                    UserPhone = user.UserPhone,
                    IsActive = user.IsActive,
                    Roles = user.Roles.Select(rol => new RolesResponseDto
                    {
                        RolId = rol.Id,
                        RolName = rol.RolName
                    }).ToList()
                };
                return new ServiceResponse<GetUsers>
                {
                    Data = userDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Usuario obtenido correctamente."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<GetUsers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener el usuario."
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

        /*public async Task<ServiceResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El usuario con ID {userId} no fue encontrado."
                    };
                }

                var existingRole = await _rolesRepository.GetByIdAsync(roleId);
                if (existingRole.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "El rol no existe."
                    };
                }

                var repositoryResult = await _userRepository.AssignRoleToUserAsync(userId, roleId);

                if (repositoryResult.OperationStatusCode == 50003)
                {
                    // Error de Negocio: Rol duplicado (Capturado del THROW 50003 de tu SP)
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El usuario ya tiene asignado este rol."
                    };
                }
                else
                

            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al asignar el rol al usuario."
                };
            }
            
        }*/
        public async Task<ServiceResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                // 1. VALIDACIÓN DE EXISTENCIA DEL USUARIO
                var userResult = await _userRepository.GetByIdAsync(userId);
                if (userResult.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El usuario con ID {userId} no fue encontrado."
                    };
                }

                // 2. VALIDACIÓN DE EXISTENCIA DEL ROL
                var roleResult = await _rolesRepository.GetByIdAsync(roleId);
                if (roleResult.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El rol con ID {roleId} no fue encontrado."
                    };
                }

                // 3. LLAMADA AL REPOSITORIO (Maneja la inserción y la validación de duplicidad)
                var repositoryResult = await _userRepository.AssignRoleToUserAsync(userId, roleId);

                // 4. MANEJO DE LA RESPUESTA DEL REPOSITORIO
                if (repositoryResult.OperationStatusCode == 0)
                {
                    // Éxito: El repositorio devolvió los nuevos roles
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = repositoryResult.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Rol asignado correctamente."
                    };
                }
                else if (repositoryResult.OperationStatusCode == 50003)
                {
                    // Error de Negocio: Rol duplicado (Capturado del THROW 50003 de tu SP)
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El usuario ya tiene asignado este rol."
                    };
                }
                else
                {
                    // Error de base de datos desconocido
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = $"Error al asignar el rol. Código: {repositoryResult.OperationStatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                // Loguear la excepción
                return new ServiceResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la solicitud."
                };
            }
        }
    }
}
