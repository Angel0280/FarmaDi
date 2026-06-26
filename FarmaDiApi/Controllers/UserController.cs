using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsersService _userService;

        public UserController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserRolesDto dto)
        {
            var serviceResponse = await _userService.RegisterUserWithRolesAsync(dto);

            if (serviceResponse.IsSuccess)
            {
                var dataResponse = new UserRolResponseDto
                {
                    UserName = serviceResponse.Data!.Users.UserName,
                    UserLastName = serviceResponse.Data.Users.UserLastName,
                    Mail = serviceResponse.Data.Users.Mail,
                    UserPhone = serviceResponse.Data.Users.UserPhone,
                    Roles = serviceResponse.Data.Roles.Select(dt => new RolesResponseDto
                    {
                        RolId = dt.Id,
                        RolName = dt.RolName
                    }).ToList()
                };

                // CORREGIDO: Se envía el ID real del usuario creado en lugar de la lista de roles
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data.Users.UserId },
                    dataResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto(); // CORREGIDO: Casing camelCase
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso relacional no encontrado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Uno o varios de los Roles proporcionados no existen." };
                    return NotFound(unsuccessfulResponse);
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Ocurrió un error en la validación de los datos";
                    return BadRequest(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error en el servidor";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) // CORREGIDO: Añadida protección de entrada para consistencia de arquitectura
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _userService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Usuario no encontrado";
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error en el servidor";
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() // CORREGIDO: Nombre homologado con los demás catálogos
        {
            var serviceResponse = await _userService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorDataBase:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error en la base de datos";
                    return StatusCode(500, unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error en el servidor";
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleAssignmentDto assignmentDto)
        {
            var result = await _userService.AssignRoleToUserAsync(assignmentDto.UserId, assignmentDto.RoleId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(result);
            }

            if (result.MessageCode == MessageCodes.Conflict)
            {
                return Conflict(result);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }
    }
    // CORREGIDO: Se eliminó el bloque masivo de código muerto/comentado al final
}