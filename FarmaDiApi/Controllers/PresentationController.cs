using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  //  [Authorize]
    public class PresentationController : ControllerBase
    {
        private readonly IPresentationService _presentationService;

        public PresentationController(IPresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPresentationDto addPresentationDto)
        {
            var serviceResponse = await _presentationService.AddAsync(addPresentationDto);

            if (serviceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = presentationDto.Id },
                    presentationDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                case MessageCodes.ErrorValidation: // CORREGIDO: Soporta ambos enums mapeando el HTTP 409 legítimo
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre de la presentación ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de una presentación" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    // CORREGIDO: Antes retornaba Conflict(409) ante un error 500
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _presentationService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var presentationDtoCollection = serviceResponse.Data!.Select(p => new GetAllPresentationDto
                {
                    Id = p.Id,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitMeasure = p.UnitMeasure,
                    IsActive = p.IsActive
                }).ToList(); // CORREGIDO: .ToList() evita doble enumeración en memoria

                var apiResponse = new ApiResponse<IEnumerable<GetAllPresentationDto>>
                {
                    Data = presentationDtoCollection,
                    Meta = new
                    {
                        TotalAmount = presentationDtoCollection.Count,
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
            if (id <= 0) // CORREGIDO: Eliminado 'id == null' por incompatibilidad con tipo int primitivo
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _presentationService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return Ok(presentationDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una presentación asociada al Id proporcionado";
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePresentationDto data)
        {
            var serviceResponse = await _presentationService.UpdateAsync(id, data);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Comentario basura de "categorías" removido
                var updatedPresentation = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedPresentation);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró presentación con el Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    // CORREGIDO: Mapeo de HTTP semántico (404 NotFound en vez de StatusCode 400)
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

            var serviceResponse = await _presentationService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return Ok(presentationDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Presentación no encontrada";
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
            var serviceResponse = await _presentationService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró presentación con el Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    // CORREGIDO: Retorna NotFound semántico en lugar de forzar StatusCode 400
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
    }
}