using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        //Constructor del controlador
        public AuthController(IAuthService users)
        {
            _authService = users;
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
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "Los datos proporcionados no son válidos";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error de validación de datos" };
                    return BadRequest(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de usuario ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de marca" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return BadRequest(unSuccessfulResponse);

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

            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Unauthorized:
                    unSuccessfulResponse.Code = "401";
                    unSuccessfulResponse.Message = "Credenciales inválidas";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "El nombre de usuario o la contraseña son incorrectos" };
                    return Unauthorized(unSuccessfulResponse);
                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrio un error inesperado";
                 
                    return StatusCode(500, unSuccessfulResponse);
            }

        }
    }
}
