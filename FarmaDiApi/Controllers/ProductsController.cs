using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
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
    public class ProductsController : ControllerBase
    {

        private readonly IProductService _ProductService;
        

        //Constructor del controlador
        public ProductsController(IProductService product)
        {
            _ProductService = product;
           
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProductDto addProductDto)
        {


            var serviceResponse = await _ProductService.AddAsync(addProductDto);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new ProductDto  
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data!.GenericName,
                    TradeName = serviceResponse!.Data.TradeName,
                    CategoryId = serviceResponse!.Data.CategoryId,
                  
                    PresentationId = serviceResponse!.Data.PresentationId,
                    ConcentrationId = serviceResponse!.Data.ConcentrationId,
                    SupplierId = serviceResponse!.Data.SupplierId,
                    BrandId = serviceResponse!.Data.BrandId,
                   
                    IsActive = serviceResponse.Data!.IsActive,

                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = productDto.ProductId },
                    productDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre del producto ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del producto" };

                    return Conflict(unSuccessfulResponse);

                    case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "Ha ocurrio un error al registar el producto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Uno o varios de los id no son válidos" };
                    return BadRequest(unSuccessfulResponse);

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
            var serviceResponse = await _ProductService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var ProductsDtoCollection = serviceResponse.Data!.Select(c => new GetAllProductsDto
                {
                    ProductId = c.ProductId,
                    GenericName = c.GenericName,
                    TradeName = c.TradeName,
                    CategoryId = c.oCategory.CategoryId,
                    CategoryName = c.oCategory.CategoryName,
                    PresentationId = c.oPresentation.Id,
                    PresentationName = c.oPresentation.Description,
                    ConcentrationId = c.oconcentration.ConcentrationId,
                    Porcentage = c.oconcentration.ConcentrationName,
                    SupplierId = c.oSupplier.SupplierId,
                    SupplierName = c.oSupplier.SupplierName,
                    BrandId = c.obrand.BrandId,
                    BrandName = c.obrand.BrandName,
                    IsActive = c.IsActive
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllProductsDto>>
                {
                    Data = ProductsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = ProductsDtoCollection.Count(),
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
            var serviceResponse = await _ProductService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new GetAllProductsDto()
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data!.GenericName,
                    TradeName = serviceResponse.Data!.TradeName,
                    CategoryId = serviceResponse.Data!.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data!.oPresentation.Id,
                    PresentationName = serviceResponse.Data!.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data!.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data!.oconcentration.ConcentrationName,
                    SupplierId = serviceResponse.Data!.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data!.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data!.obrand.BrandId,
                    BrandName = serviceResponse.Data!.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró un producto asociado al Id proporcionado",
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dataProduct)
        {

           
            var serviceResponse = await _ProductService.UpdateAsync(id, dataProduct);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updatedproduct = new ProductDto
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data!.GenericName,
                    TradeName = serviceResponse.Data!.TradeName,
                    CategoryId = serviceResponse.Data!.oCategory.CategoryId,
                    PresentationId = serviceResponse.Data!.oPresentation.Id,
                    ConcentrationId = serviceResponse.Data!.oconcentration.ConcentrationId,
                  
                    SupplierId = serviceResponse.Data!.oSupplier.SupplierId,
                 
                    BrandId = serviceResponse.Data!.obrand.BrandId,
               
                    IsActive = serviceResponse.Data.IsActive,
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedproduct);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "No encontrado";
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

            var serviceResponse = await _ProductService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var productDto = new GetAllProductsDto()
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data!.GenericName,
                    TradeName = serviceResponse.Data!.TradeName,
                    CategoryId = serviceResponse.Data!.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data!.oPresentation.Id,
                    PresentationName = serviceResponse.Data!.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data!.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data!.oconcentration.ConcentrationName,
                    SupplierId = serviceResponse.Data!.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data!.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data!.obrand.BrandId,
                    BrandName = serviceResponse.Data!.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Producto no encontrada";
                    unSuccessfulResponse.Details = new { Error = "El recurso solicitado no está disponible en el servidor" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }




        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _ProductService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
             
                return Ok(new
                {
                    ProductId = serviceResponse.Data!.ProductId,
                    GenericName = serviceResponse.Data!.GenericName,
                    TradeName = serviceResponse.Data!.TradeName,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    PresentatioId = serviceResponse.Data!.PresentationId,
                    ConcentrationId = serviceResponse.Data!.ConcentrationId,

                    SupplierId = serviceResponse.Data!.SupplierId,

                    BrandId = serviceResponse.Data!.BrandId,


                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un producto con el Id proporcionado";
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

            var serviceResponse = await _ProductService.GetProductsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess)
            {
                var (products, totalCount) = serviceResponse.Data;

                var ProductsDtoCollection = products.Select(c => new GetAllProductsDto
                {
                    ProductId = c.ProductId,
                    GenericName = c.GenericName,
                    TradeName = c.TradeName,
                    CategoryId = c.oCategory.CategoryId,
                    CategoryName = c.oCategory.CategoryName,
                    PresentationId = c.oPresentation.Id,
                    PresentationName = c.oPresentation.Description,
                    ConcentrationId = c.oconcentration.ConcentrationId,
                    Porcentage = c.oconcentration.ConcentrationName,
                    SupplierId = c.oSupplier.SupplierId,
                    SupplierName = c.oSupplier.SupplierName,
                    BrandId = c.obrand.BrandId,
                    BrandName = c.obrand.BrandName,
                    IsActive = c.IsActive
                }).ToList();

                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<IEnumerable<GetAllProductsDto>>
                {
                    Data = ProductsDtoCollection,
                    Meta = new GetProductPagedDTO
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
