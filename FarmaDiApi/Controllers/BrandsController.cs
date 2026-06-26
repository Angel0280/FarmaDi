using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandsService _brandsService;

        public BrandsController(IBrandsService brandsService)
        {
            _brandsService = brandsService;
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
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = brandDto.BrandId },
                    brandDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre de la marca ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de marca" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _brandsService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var brandsDtoCollection = serviceResponse.Data!.Select(b => new GetAllBrandsDto
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    BrandDescription = b.Description,
                    IsActive = b.IsActive
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<GetAllBrandsDto>>
                {
                    Data = brandsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = brandsDtoCollection.Count,
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

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una marca asociada al Id proporcionado";
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandDto dataBrand)
        {
            var serviceResponse = await _brandsService.UpdateAsync(id, dataBrand);

            if (serviceResponse.IsSuccess)
            {
                var updatedBrand = new BrandDto
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedBrand);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró Marca con el Id proporcionado";
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

            var serviceResponse = await _brandsService.GetByNameAsync(name);

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
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Marca no encontrada";
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
            var serviceResponse = await _brandsService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró Marca con el Id proporcionado";
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

            var serviceResponse = await _brandsService.GetBrandsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess)
            {
                var (brands, totalCount) = serviceResponse.Data;
                var brandsCollection = brands.Select(b => new GetAllBrandsDto
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    BrandDescription = b.Description,
                    IsActive = b.IsActive
                }).ToList();

                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<IEnumerable<GetAllBrandsDto>>
                {
                    Data = brandsCollection,
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