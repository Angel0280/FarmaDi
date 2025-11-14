using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace FarmaDiBusiness.Services
{

    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IRolService _roleService;
        private readonly IConfiguration _configuration;
        public AuthService(IAuthRepository authRepository, IRolService rolesRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _roleService = rolesRepository;
            _configuration = configuration;
        }

        private string GenerateTokenJWT(Users users, IEnumerable<string> roles)
        {
            //Acceder a la configuracion del JWTSettings que esta en appsettings.json
            // que contiene la clave secreta, el publico y la audiencia
            var secretKey = _configuration["JWTSettings:SecretKey"];
            var issuer = _configuration["JWTSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"];

            //Crear los claims(afirmaciones) o info clave que se van a incluir en el token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,users.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name,users.UserName),
                new Claim(JwtRegisteredClaimNames.Email,users.Mail),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            //Agregar los roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Encriptar la llave secreta 
            // codificando en bytes
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credencials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Definir el tiempo de expiracion del token
            var expiries = DateTime.UtcNow.AddHours(3);

            //Crear el token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                signingCredentials: credencials,
                expires: expiries


                ); 
            //Serializar el token
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token); 

        }

        public async Task<ServiceResponse<Users>> RegisterAsync(AddUserDto newuser)
        {
            try
            {
               /*var existing = await _authRepository.GetByUserNameAsync(newuser.UserName);
                if (existing.Data != null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario registrado con el mismo nombre."
                    };
                }
               
                /*var roleResponse = await _roleService.GetRolByIdAsync(newuser.RolId);
                if (roleResponse.Data == null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un rol que coincida con el id que se quiere insertar ."
                    };
                }
                var mailExisting = await _authRepository.GetByEmailAsync(newuser.Mail);
                if (mailExisting.Data == null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario registrado con el mismo correo electrónico."
                    };
                }*/
                var userToRegister = new Users
                {
                    UserName = newuser.UserName,
                    UserLastName = newuser.UserLastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newuser.Password),
                    Mail = newuser.Mail,
                    UserPhone = newuser.UserPhone
                    
                };
                var result = await _authRepository.RegisterAsync(userToRegister);
                return new ServiceResponse<Users>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Usuario registrado correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error al registrar el usuario."
                };

            }
        }
        public async Task<ServiceResponse<Users>> GetByEmailAsync(string mail)
        {

            var result = await _authRepository.GetByEmailAsync(mail);
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
                    message = "No se encontró un usuario que corresponda al correo proporcionado";
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


        public async Task<ServiceResponse<Users>> GetByNameAsync(string name)
        {
            var result = await _authRepository.GetByUserNameAsync(name);
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

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto)
        {
            try
            {
                var existentUser = await _authRepository.GetByUserNameAsync(loginRequestDto.UserName);
                if (existentUser.Data!.UserId == 0 && existentUser.Data!.UserName.IsNullOrEmpty())
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = "El usuario con el nombre proporcionado no existe."
                    };

                }

                var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existentUser.Data!.PasswordHash);
                if (!isPasswordValid)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = "La contraseña proporcionada es incorrecta."
                    };
                }

                var roles = await _authRepository.GetRolesByUserIdAsync(existentUser.Data!.UserId);
                //Generar el token JWT aquí

                var token = GenerateTokenJWT(existentUser.Data!, roles.Data!);
                var loginResponse = new LoginResponseDto
                {
                    Id = existentUser.Data!.UserId,
                    UserName = existentUser.Data!.UserName,
                    Email = existentUser.Data!.Mail,
                    Token = token,
                    Roles = roles.Data!

                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Data = loginResponse,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Login correcto"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio algo inesperado",
                };
            }
 
        }


    }
}
