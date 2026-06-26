using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductBatchesDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductBatchesController : ControllerBase
    {
        private readonly IProductBatchesService _productBatchesService;

        public ProductBatchesController(IProductBatchesService productBatchesService)
        {
            _productBatchesService = productBatchesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _productBatchesService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Renombrado de variable y conversión a lista para optimizar doble enumeración
                var batchesDtoCollection = serviceResponse.Data!.Select(b => new GetAllProductBatchesDto
                {
                    BatchId = b.Id,
                    BatchNumer = b.BatchNumer,
                    ManufacturingDate = b.ManufacturingDate,
                    ExpirationDate = b.ExpirationDate,
                    Quantity = b.Quantity,
                    ProductId = b.oProduct.ProductId,
                    ProductGenericName = b.oProduct.GenericName,
                    ProductTradeName = b.oProduct.TradeName,
                    IsActive = b.IsActive,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<GetAllProductBatchesDto>>
                {
                    Data = batchesDtoCollection,
                    Meta = new
                    {
                        TotalAmount = batchesDtoCollection.Count,
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
            if (id <= 0) // CORREGIDO: Se eliminó 'id == null' por ser un int primitivo
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _productBatchesService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Se renombró la variable (antes llamada inventoryLossDto erróneamente)
                var productBatchDto = new GetAllProductBatchesDto
                {
                    BatchId = serviceResponse.Data!.Id,
                    BatchNumer = serviceResponse.Data.BatchNumer,
                    ManufacturingDate = serviceResponse.Data.ManufacturingDate,
                    ExpirationDate = serviceResponse.Data.ExpirationDate,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    ProductGenericName = serviceResponse.Data.oProduct.GenericName,
                    ProductTradeName = serviceResponse.Data.oProduct.TradeName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productBatchDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró un lote asociado al Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no esperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}