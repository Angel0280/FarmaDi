using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddRolDto addRolDto)
        {
            var serviceResponse = await _rolService.AddRolAsync(addRolDto);

            if (serviceResponse.IsSuccess)
            {
                var rolDto = new RolDto
                {
                    RolId = serviceResponse.Data!.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetByName),
                    new { name = rolDto.RolName },
                    rolDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre del rol ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del rol" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    // CORREGIDO: Antes retornaba BadRequest(400) para un error interno 500
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _rolService.GetAllRolsAsync();

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Se añade .ToList() para evitar la doble enumeración diferida de LINQ
                var rolesDtoCollection = serviceResponse.Data!.Select(r => new GetAllRolsDto
                {
                    Id = r.Id,
                    RolName = r.RolName,
                    IsActive = r.IsActive
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<GetAllRolsDto>>
                {
                    Data = rolesDtoCollection,
                    Meta = new
                    {
                        TotalAmount = rolesDtoCollection.Count,
                        message = serviceResponse.Message
                    }
                };
                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) // CORREGIDO: Removido 'id == null' por incompatibilidad con int primitivo
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _rolService.GetRolByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var rolDto = new GetAllRolsDto
                {
                    Id = serviceResponse.Data!.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };

                return Ok(rolDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un rol asociado al Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no esperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto dataRol)
        {
            var serviceResponse = await _rolService.UpdateRolAsync(id, dataRol);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Comentario basura de categorías removido
                var updatedRol = new RolDto
                {
                    RolId = serviceResponse.Data!.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };
                return Ok(updatedRol);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un rol con el Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            if (name.IsNullOrEmpty())
            {
                unsuccessfulResponse.Code = "400";
                unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                unsuccessfulResponse.Details = new { Error = "El nombre no puede ser nulo o vacío" };
                return BadRequest(unsuccessfulResponse);
            }

            var serviceResponse = await _rolService.GetRolByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var rolDto = new GetAllRolsDto
                {
                    Id = serviceResponse.Data!.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };

                return Ok(rolDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Rol no encontrado";
                    unsuccessfulResponse.Details = new { Error = "El recurso solicitado no está disponible en el servidor" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _rolService.SetRolStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Comentarios de marcas y categorías removidos
                return Ok(new
                {
                    serviceResponse.Data!.Id,
                    serviceResponse.Data.RolName,
                    serviceResponse.Data.IsActive,
                    serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un rol con el Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}