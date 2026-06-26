using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] AddUserDto newuser)
        {
            var serviceResponse = await _authService.RegisterAsync(newuser);

            if (serviceResponse.IsSuccess)
            {
                var userDto = new AddUserDto
                {
                    UserName = serviceResponse.Data!.UserName,
                    UserLastName = serviceResponse.Data!.UserLastName,
                    Mail = serviceResponse.Data!.Mail,
                    UserPhone = serviceResponse.Data!.UserPhone
                };

                return CreatedAtAction(
                    nameof(Register),
                    new { id = serviceResponse.Data!.UserId },
                    userDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto(); // CORREGIDO: Casing camelCase
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Los datos proporcionados no son válidos";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error de validación de datos" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre de usuario o correo ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar la información de una cuenta de usuario" }; // CORREGIDO: Texto de "marcas" removido
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    // CORREGIDO: Antes retornaba BadRequest forzado para un error interno 500
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var serviceResponse = await _authService.LoginAsync(loginRequest);

            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:       // NUEVO: Captura el estado seguro si el username no existe en la BD
                case MessageCodes.Unauthorized:   // Captura si la contraseña es incorrecta
                    unsuccessfulResponse.Code = "401";
                    unsuccessfulResponse.Message = "Credenciales inválidas";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "El nombre de usuario o la contraseña son incorrectos" };
                    return Unauthorized(unsuccessfulResponse); // HTTP 401 impecable

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}