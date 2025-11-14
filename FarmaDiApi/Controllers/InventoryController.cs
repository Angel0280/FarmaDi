using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {

        private readonly IInventoryService _inventoryService;

        //Constructor del controlador
        public InventoryController(IInventoryService inventory)
        {
            _inventoryService = inventory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _inventoryService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                //mapeo de los datos recibidos a la estructura del DTO a enviar
                //En este caso mapear la estructura brand a brandDTO usando LINQ
                var inventoryDtoCollection = serviceResponse.Data!.Select(c => new GetAllInventoryDto
                {
                    InventoryId = c.InventoryId,
                    ProductId = c.oproduct.ProductId,
                    ProductGenericname = c.oproduct.GenericName,
                    SalePrice = c.SalePrice,
                    PurchasePrice = c.PurchasePrice,
                    CriticalStock = c.CriticalStock
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllInventoryDto>>
                {
                    Data = inventoryDtoCollection,
                    Meta = new
                    {
                        TotalAmount = inventoryDtoCollection.Count(),
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
            var serviceResponse = await _inventoryService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var brandDto = new GetAllInventoryDto()
                {
                    InventoryId = serviceResponse.Data!.InventoryId,
                    ProductId = serviceResponse.Data.oproduct.ProductId,
                    ProductGenericname = serviceResponse.Data.oproduct.GenericName,
                    SalePrice = serviceResponse.Data.SalePrice,
                    PurchasePrice = serviceResponse.Data.PurchasePrice,
                    CriticalStock = serviceResponse.Data.CriticalStock,
                };

                return Ok(brandDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = $"No se encontró ningun dato relacionado al Id { id }",
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

    }
}
