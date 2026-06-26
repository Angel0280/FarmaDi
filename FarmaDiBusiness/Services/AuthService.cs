using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FarmaDiBusiness.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        private string GenerateTokenJWT(Users user, IEnumerable<string> roles)
        {
            var secretKey = _configuration["JWTSettings:SecretKey"]!;
            var issuer = _configuration["JWTSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Mail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(3);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                signingCredentials: credentials,
                expires: expires
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<Users>> RegisterAsync(AddUserDto newUser)
        {
            try
            {
                // 1. CORREGIDO: Reactivación y blindaje de la validación de duplicidad de nombre de usuario
                var existingUser = await _authRepository.GetByUserNameAsync(newUser.UserName);
                if (existingUser?.Data != null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.FileExisting,
                        Message = "Ya existe un usuario registrado con el mismo nombre."
                    };
                }

                // 2. CORREGIDO: Reactivación y blindaje de la validación de duplicidad de correo electrónico
                var existingEmail = await _authRepository.GetByEmailAsync(newUser.Mail);
                if (existingEmail?.Data != null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario registrado con el mismo correo electrónico."
                    };
                }

                var userToRegister = new Users
                {
                    UserName = newUser.UserName,
                    UserLastName = newUser.UserLastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    Mail = newUser.Mail,
                    UserPhone = newUser.UserPhone
                };

                var result = await _authRepository.RegisterAsync(userToRegister);

                return new ServiceResponse<Users>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Usuario registrado correctamente."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar el usuario."
                };
            }
        }

        public async Task<ServiceResponse<Users>> GetByEmailAsync(string mail)
        {
            try
            {
                // CORREGIDO: Todo el flujo envuelto en try-catch estratégico
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

                var messageCode = MessageCodes.Success;
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

                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar el usuario por correo electrónico."
                };
            }
        }

        public async Task<ServiceResponse<Users>> GetByNameAsync(string name)
        {
            try
            {
                // CORREGIDO: Todo el flujo envuelto en try-catch estratégico
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

                var messageCode = MessageCodes.Success;
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

                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar el usuario por su nombre."
                };
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto)
        {
            try
            {
                // 1. CORREGIDO: Verificación real y segura de la existencia del objeto de datos
                var existentUser = await _authRepository.GetByUserNameAsync(loginRequestDto.UserName);

                if (existentUser?.Data == null || existentUser.OperationStatusCode != 0)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, // Evaluado en AuthController para emitir HTTP 401
                        Message = "El nombre de usuario o la contraseña son incorrectos."
                    };
                }

                // CORREGIDO: Se eliminó el bloque condicional muerto/incongruente que evaluaba (UserId == 0)

                // 2. Validación criptográfica de la contraseña ingresada contra el Hash
                var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existentUser.Data.PasswordHash);
                if (!isPasswordValid)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized, // Evaluado en AuthController para emitir HTTP 401
                        Message = "El nombre de usuario o la contraseña son incorrectos." // CORREGIDO: Mensaje OWASP uniforme
                    };
                }

                // 3. Recuperación de los roles asociados al identificador del usuario
                var roles = await _authRepository.GetRolesByUserIdAsync(existentUser.Data.UserId);

                // 4. Emisión y serialización del token JWT
                var token = GenerateTokenJWT(existentUser.Data, roles.Data ?? new List<string>());

                var loginResponse = new LoginResponseDto
                {
                    Id = existentUser.Data.UserId,
                    UserName = existentUser.Data.UserName,
                    Email = existentUser.Data.Mail,
                    Token = token,
                    Roles = roles.Data ?? new List<string>()
                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Data = loginResponse,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Login correcto."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el inicio de sesión."
                };
            }
        }
    }
}