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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoryService;

        //Constructor del controlador
        public CategoriesController(ICategoriesService category)
        {
            _categoryService = category;
        }


        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AddCategoryDto addCategoryDto)
        {
            var serviceResponse = await _categoryService.AddAsync(addCategoryDto);

            if (serviceResponse.IsSuccess)
            {
                var CategoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    Description = serviceResponse!.Data.CategoryDescription,
                    IsActive = serviceResponse.Data!.IsActive,

                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = CategoryDto.CategoryId },
                    CategoryDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la  categoria ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de la categoria" };

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
            var serviceResponse = await _categoryService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                //mapeo de los datos recibidos a la estructura del DTO a enviar
                
                var DtoCollection = serviceResponse.Data!.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllCategoriesDto>>
                {
                    Data = DtoCollection,
                    Meta = new
                    {
                        TotalAmount = DtoCollection.Count(),
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
            var serviceResponse = await _categoryService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                   CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(categoryDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró una categoria asociada al Id proporcionado",
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto category)
        {
            var serviceResponse = await _categoryService.UpdateAsync(id, category);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updated = new CategoryDto
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    Description = serviceResponse.Data!.CategoryDescription,
                    IsActive = serviceResponse.Data!.IsActive,
                    
                    
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updated);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró una categoria  con el Id proporcionado";
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

            var ServiceResponse = await _categoryService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var categoryDto = new CategoryDto()
                {
                    CategoryId = ServiceResponse.Data!.CategoryId,
                    CategoryName = ServiceResponse.Data.CategoryName,
                    Description = ServiceResponse.Data.CategoryDescription,
                    IsActive = ServiceResponse.Data.IsActive,
                };

                return Ok(categoryDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "categoria no encontrada";
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
            var serviceResponse = await _categoryService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                
                return Ok(new
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName ,
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
                    unSuccessfulResponse.Message = "No se encontró categoria con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);


                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 10;
            if (limit > 100) limit = 100;

            var serviceResponse = await _categoryService.GetCategoriesPaged(page, limit);
            if (serviceResponse.IsSuccess)
            {
                var (categories, totalCount) = serviceResponse.Data;
                var CategoriesCollection = categories.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive,
                }).ToList();

                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<IEnumerable<GetAllCategoriesDto>>
                {
                    Data = CategoriesCollection,
                    Meta = new GetPagedDto
                    {
                        TotalItems = totalCount,
                        TotalPages = totalPages,
                        CurrentPage = page,
                        ItemsPerPage = limit
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


    }
}
