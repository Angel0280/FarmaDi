using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProductDto addProductDto)
        {
            var serviceResponse = await _productService.AddAsync(addProductDto);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new ProductDto
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.PresentationId,
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = productDto.ProductId },
                    productDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre del producto ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del producto" };
                    return Conflict(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Ha ocurrido un error al registrar el producto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Uno o varios de los id no son válidos" };
                    return BadRequest(unsuccessfulResponse);

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
            var serviceResponse = await _productService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Iterador cambiado a 'p' y agregado .ToList() para optimizar conteo
                var productsDtoCollection = serviceResponse.Data!.Select(p => new GetAllProductsDto
                {
                    ProductId = p.ProductId,
                    GenericName = p.GenericName,
                    TradeName = p.TradeName,
                    CategoryId = p.oCategory.CategoryId,
                    CategoryName = p.oCategory.CategoryName,
                    PresentationId = p.oPresentation.Id,
                    PresentationName = p.oPresentation.Description,
                    ConcentrationId = p.oconcentration.ConcentrationId,
                    Porcentage = p.oconcentration.Volume,
                    ConcentrationValue = p.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = p.oSupplier.SupplierId,
                    SupplierName = p.oSupplier.SupplierName,
                    BrandId = p.obrand.BrandId,
                    BrandName = p.obrand.BrandName,
                    IsActive = p.IsActive
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<GetAllProductsDto>>
                {
                    Data = productsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = productsDtoCollection.Count,
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

            var serviceResponse = await _productService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new GetAllProductsDto
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data.oPresentation.Id,
                    PresentationName = serviceResponse.Data.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data.oconcentration.Volume,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = serviceResponse.Data.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data.obrand.BrandId,
                    BrandName = serviceResponse.Data.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un producto asociado al Id proporcionado";
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dataProduct)
        {
            var serviceResponse = await _productService.UpdateAsync(id, dataProduct);

            if (serviceResponse.IsSuccess)
            {
                var updatedProduct = new ProductDto
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedProduct);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    // CORREGIDO: Sincronización de códigos semánticos para error de recurso no encontrado (404)
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No encontrado";
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

            var serviceResponse = await _productService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new GetAllProductsDto
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data.oPresentation.Id,
                    PresentationName = serviceResponse.Data.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data.oconcentration.Volume,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = serviceResponse.Data.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data.obrand.BrandId,
                    BrandName = serviceResponse.Data.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Producto no encontrado";
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
            var serviceResponse = await _productService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.PresentationId,
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un producto con el Id proporcionado";
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

            var serviceResponse = await _productService.GetProductsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess)
            {
                var (products, totalCount) = serviceResponse.Data;

                var productsDtoCollection = products.Select(p => new GetAllProductsDto
                {
                    ProductId = p.ProductId,
                    GenericName = p.GenericName,
                    TradeName = p.TradeName,
                    CategoryId = p.oCategory.CategoryId,
                    CategoryName = p.oCategory.CategoryName,
                    PresentationId = p.oPresentation.Id,
                    PresentationName = p.oPresentation.Description,
                    ConcentrationId = p.oconcentration.ConcentrationId,
                    Porcentage = p.oconcentration.Volume,
                    ConcentrationValue = p.ConcentrationValue, // NUEVO: Mapeo de la característica
                    SupplierId = p.oSupplier.SupplierId,
                    SupplierName = p.oSupplier.SupplierName,
                    BrandId = p.obrand.BrandId,
                    BrandName = p.obrand.BrandName,
                    IsActive = p.IsActive
                }).ToList();

                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<IEnumerable<GetAllProductsDto>>
                {
                    Data = productsDtoCollection,
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