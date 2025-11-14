using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.RolsDto;
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
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        //Constructor del controlador
        public RolesController(IRolService rol)
        {
            _rolService = rol;
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AddRolDto addRolDto)
        {
            var serviceResponse = await _rolService.AddRolAsync(addRolDto);

            if (serviceResponse.IsSuccess)
            {
                var rolDto = new RolDto
                {
                    // ahora si mapeamos la respuesta que viene de servicio, repo--servicio--controllador
                    RolId = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data!.RolName,
                    IsActive = serviceResponse.Data.IsActive,
                    

                };
                return CreatedAtAction(
                    nameof(GetByName),
                    new { name = rolDto.RolName},
                    rolDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre del rol ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del rol" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return BadRequest(unSuccessfulResponse);



            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _rolService.GetAllRolsAsync();

            if (serviceResponse.IsSuccess)
            {
                
                var rolesDtoCollection = serviceResponse.Data!.Select(c => new GetAllRolsDto
                {
                    Id = c.Id,
                    RolName = c.RolName,
                    IsActive = c.IsActive
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllRolsDto>>
                {
                    Data = rolesDtoCollection,
                    Meta = new
                    {
                        TotalAmount =rolesDtoCollection.Count(),
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
            if (id <= 0 || id == null)
            {
                var response = new UnsuccessfulResponseDto()
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
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró un rol asociado al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };

                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "500",
                        Message = "Ocurrió un error",
                        Details = new { info = serviceResponse.Message ?? "Error interno no esperado" }
                    };

                    return StatusCode(500, unsuccessfulResponse);

            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto dataRol)
        {
            // Se invoca al servicio confiando en que el framework ya validó el DTO
            var serviceResponse = await _rolService.UpdateRolAsync(id, dataRol);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updatedRol = new RolDto
                {
                    RolId = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data!.RolName,
                    IsActive = serviceResponse.Data!.IsActive
                };
                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedRol);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un rol con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            if (name.IsNullOrEmpty())
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                unSuccessfulResponse.Details = new { Error = "El nombre no puede ser nulo o vacío" };

                return BadRequest(unSuccessfulResponse);

            }

            var ServiceResponse = await _rolService.GetRolByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var rolDto = new GetAllRolsDto()
                {
                    Id = ServiceResponse.Data!.Id,
                    RolName = ServiceResponse.Data.RolName,
                    IsActive = ServiceResponse.Data.IsActive
                };

                return Ok(rolDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Rol no encontrado";
                    unSuccessfulResponse.Details = new { Error = "El recurso solicitado no está disponible en el servidor" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrió un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            // Llama al servicio que maneja la lógica de activación/desactivación
            var serviceResponse = await _rolService.SetRolStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                // Devuelve la marca actualizada con mensaje
                return Ok(new
                {
                    serviceResponse.Data!.Id,
                    serviceResponse.Data.RolName,
                    serviceResponse.Data.IsActive,
                    serviceResponse.Message
                });
            }

            // Devuelve error si no se encontró la categoría o hubo problema
            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un rol con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);


                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}
