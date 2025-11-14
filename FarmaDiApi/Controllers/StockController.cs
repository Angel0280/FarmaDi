using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.StockDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _Service;

        //Constructor del controlador
        public StockController(IStockService stock)
        {
            _Service = stock;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _Service.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var stockDtoCollection = serviceResponse.Data!.Select(c => new StockDto
                {
                    StockId = c.Id,
                    BachtId = c.BatchId.Id,
                    BatchNumber = c.BatchId.BatchNumer,
                    AvailableQuantity = c.AvailableQuantity,
                   
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<StockDto>>
                {
                    Data = stockDtoCollection,
                    Meta = new
                    {
                        TotalAmount = stockDtoCollection.Count(),
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
            var serviceResponse = await _Service.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var stockDto = new StockDto
                {
                    StockId = serviceResponse.Data!.Id,
                    BachtId = serviceResponse.Data.BatchId.Id,
                    BatchNumber = serviceResponse.Data.BatchId.BatchNumer,
                    AvailableQuantity = serviceResponse.Data.AvailableQuantity,
                };

                return Ok(stockDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró ningún registro asociado al Id proporcionado",
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
