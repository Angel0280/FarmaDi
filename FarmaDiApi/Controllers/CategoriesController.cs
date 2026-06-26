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
   // [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoryService;

        public CategoriesController(ICategoriesService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCategoryDto addCategoryDto)
        {
            var serviceResponse = await _categoryService.AddAsync(addCategoryDto);

            if (serviceResponse.IsSuccess)
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = categoryDto.CategoryId },
                    categoryDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre de la categoría ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de la categoría" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    // CORREGIDO: Antes retornaba BadRequest (400) con código interno 500
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _categoryService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var dtoCollection = serviceResponse.Data!.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<GetAllCategoriesDto>>
                {
                    Data = dtoCollection,
                    Meta = new
                    {
                        TotalAmount = dtoCollection.Count,
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
            if (id <= 0)
            {
                var response = new UnsuccessfulResponseDto
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

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una categoría asociada al Id proporcionado";
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto category)
        {
            var serviceResponse = await _categoryService.UpdateAsync(id, category);

            if (serviceResponse.IsSuccess)
            {
                var updated = new CategoryDto
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updated);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una categoría con el Id proporcionado";
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

            var serviceResponse = await _categoryService.GetByNameAsync(name);

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
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Categoría no encontrada";
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
            var serviceResponse = await _categoryService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró categoría con el Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unsuccessfulResponse);
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
                var categoriesCollection = categories.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive,
                }).ToList();

                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<IEnumerable<GetAllCategoriesDto>>
                {
                    Data = categoriesCollection,
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
                case MessageCodes.NotFound:
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