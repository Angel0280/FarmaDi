using FarmaDiBusiness.DTOs;
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
    public class PresentationController : ControllerBase
    {

        private readonly IPresentationService _presentationService;

        //Constructor del controlador
        public PresentationController(IPresentationService presentation)
        {
            _presentationService = presentation;
        }



        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AddPresentationDto addPresentationDto)
        {
            var serviceResponse = await _presentationService.AddAsync(addPresentationDto);

            if (serviceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse!.Data.Description,
                    Quantity = serviceResponse!.Data.Quantity,
                    UnitMeasure = serviceResponse!.Data.UnitMeasure,

                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = presentationDto.Id },
                    presentationDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la  presentación ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de una presentación" };

                    return BadRequest(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return Conflict(unSuccessfulResponse);



            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _presentationService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                //mapeo de los datos recibidos a la estructura del DTO a enviar
                //En este caso mapear la estructura brand a brandDTO usando LINQ
                var presentationDtoCollection = serviceResponse.Data!.Select(c => new GetAllPresentationDto
                {
                    Id = c.Id,
                    Description = c.Description,
                    Quantity = c.Quantity,
                    UnitMeasure = c.UnitMeasure,
                    IsActive = c.IsActive
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllPresentationDto>>
                {
                    Data = presentationDtoCollection,
                    Meta = new
                    {
                        TotalAmount = presentationDtoCollection.Count(),
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
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró una presentación asociada al Id proporcionado",
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePresentationDto data)
        {
           
            var serviceResponse = await _presentationService.UpdateAsync(id, data);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updatedBrand = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    Description = serviceResponse.Data!.Description,
                    Quantity = serviceResponse.Data!.Quantity,
                    UnitMeasure = serviceResponse.Data!.UnitMeasure,
                    IsActive = serviceResponse.Data!.IsActive,
                    
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedBrand);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró presentación con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return StatusCode(400, unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return StatusCode(409, unSuccessfulResponse);

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

            var ServiceResponse = await _presentationService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto()
                {
                    Id = ServiceResponse.Data!.Id,
                    Description = ServiceResponse.Data.Description,
                    Quantity = ServiceResponse.Data.Quantity,
                    UnitMeasure = ServiceResponse.Data.UnitMeasure,
                };

                return Ok(presentationDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "presentation no encontrada";
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



            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró presentación con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return StatusCode(400, unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return StatusCode(409, unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }



    }
}
