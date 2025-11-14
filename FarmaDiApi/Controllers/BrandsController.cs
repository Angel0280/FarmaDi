using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandsService _brandsService;

        //Constructor del controlador
        public BrandsController(IBrandsService brand)
        {
            _brandsService = brand;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddBrandDto addBrandDto)
        {
            var serviceResponse = await _brandsService.AddAsync(addBrandDto);

            if (serviceResponse.IsSuccess)
            {
                var brandDto = new BrandDto
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    Name = serviceResponse.Data!.BrandName,
                    Description = serviceResponse!.Data.Description,
                    IsActive = serviceResponse.Data!.IsActive,

                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = brandDto.BrandId },
                    brandDto); 


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la  marca ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de marca" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };


                    // aqui hice un cambio en badRequest 
                    return StatusCode(500, unSuccessfulResponse);



            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _brandsService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                //mapeo de los datos recibidos a la estructura del DTO a enviar
                //En este caso mapear la estructura brand a brandDTO usando LINQ
                var brandsDtoCollection = serviceResponse.Data!.Select(c => new GetAllBrandsDto
                {
                    BrandId = c.BrandId,
                    BrandName = c.BrandName,
                    BrandDescription = c.Description,
                    IsActive = c.IsActive
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllBrandsDto>>
                {
                    Data = brandsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = brandsDtoCollection.Count(),
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
            var serviceResponse = await _brandsService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var brandDto = new BrandDto
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(brandDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró una marca asociada al Id proporcionado",
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandDto dataBrand)
        {
            // Nota: las validaciones de formato al campo BrandName, Description e IsActive
            // del UpdateBrandDto las realiza el framework considerando las DataAnnotations.

            // Se invoca al servicio confiando en que el framework ya validó el DTO
            var serviceResponse = await _brandsService.UpdateAsync(id, dataBrand);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updatedBrand = new BrandDto
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    Name = serviceResponse.Data!.BrandName,
                    Description = serviceResponse.Data!.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedBrand);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró Marca con el Id proporcionado";
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

            var ServiceResponse = await _brandsService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var brandDto = new BrandDto()
                {
                    BrandId = ServiceResponse.Data!.BrandId,
                    Name = ServiceResponse.Data.BrandName,
                    Description = ServiceResponse.Data.Description,
                    IsActive = ServiceResponse.Data.IsActive,
                };

                return Ok(brandDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Marca no encontrada";
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
            var serviceResponse = await _brandsService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                // Devuelve la marca actualizada con mensaje
                return Ok(new 
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            // Devuelve error si no se encontró la categoría o hubo problema
            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró Marca con el Id proporcionado";
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