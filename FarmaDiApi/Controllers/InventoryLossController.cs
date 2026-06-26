using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryLossController : ControllerBase
    {
        private readonly IInventoryLossService _inventoryLossService;

        public InventoryLossController(IInventoryLossService inventoryLossService)
        {
            _inventoryLossService = inventoryLossService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _inventoryLossService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var inventoryLossDtoCollection = serviceResponse.Data!.Select(il => new GetAllInventoryLossDto
                {
                    LowId = il.LowId,
                    BatchId = il.oBatch.Id,
                    BatchNumber = il.oBatch.BatchNumer,
                    Quantity = il.Quantity,
                    ProductId = il.oProduct.ProductId,
                    ProductGenericName = il.oProduct.GenericName,
                    ProductTradeName = il.oProduct.TradeName,
                    UserId = il.UserId,
                    UserName = il.UserName,
                    Reason = il.Reason,
                }).ToList(); // CORREGIDO: Evita doble enumeración al contar y mapear

                var apiResponse = new ApiResponse<IEnumerable<GetAllInventoryLossDto>>
                {
                    Data = inventoryLossDtoCollection,
                    Meta = new
                    {
                        TotalAmount = inventoryLossDtoCollection.Count,
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
            if (id <= 0) // CORREGIDO: Removido 'id == null' por ser un tipo int primitivo
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _inventoryLossService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var inventoryLossDto = new GetAllInventoryLossDto
                {
                    LowId = serviceResponse.Data!.LowId,
                    BatchId = serviceResponse.Data.oBatch.Id,
                    BatchNumber = serviceResponse.Data.oBatch.BatchNumer,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    ProductGenericName = serviceResponse.Data.oProduct.GenericName,
                    ProductTradeName = serviceResponse.Data.oProduct.TradeName,
                    UserId = serviceResponse.Data.UserId,
                    UserName = serviceResponse.Data.UserName,
                    Reason = serviceResponse.Data.Reason,
                };

                return Ok(inventoryLossDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una baja asociada al Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no esperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddInventoryLossDto addInventoryLossDto)
        {
            if (addInventoryLossDto.Quantity <= 0)
            {
                var unsuccessfulResponseQuantity = new UnsuccessfulResponseDto
                {
                    Code = "400", // CORREGIDO: Las validaciones de payload de entrada corresponden a BadRequest (400)
                    Message = "Cantidad inferior a la requerida",
                    Details = new { info = "La cantidad requerida para la operación debe ser mayor a 0" }
                };
                return BadRequest(unsuccessfulResponseQuantity);
            }

            // CORREGIDO: Se eliminó la variable muerta 'var id = addInventoryLossDto.BatchId;'

            var serviceResponse = await _inventoryLossService.AddAsync(addInventoryLossDto);

            if (serviceResponse.IsSuccess)
            {
                var inventoryLossDto = new InventoryLossDto
                {
                    LowId = serviceResponse.Data!.LowId,
                    BatchId = serviceResponse.Data.oBatch.Id,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    UserId = serviceResponse.Data.UserId,
                    Reason = serviceResponse.Data.Reason,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = inventoryLossDto.LowId },
                    inventoryLossDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El nombre de la baja ya existe";
                    unsuccessfulResponse.Details = new { info = "No se puede duplicar información de la baja" };
                    return Conflict(unsuccessfulResponse);

                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso no encontrado";
                    unsuccessfulResponse.Details = new { info = "No existe un lote que corresponda al id brindado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    // CORREGIDO: Antes retornaba BadRequest con código interno 500
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] string value)
        {
            // CORREGIDO: Estructurado para cumplir con las buenas prácticas de respuestas REST de la aplicación
            var mensaje = "Estamos mejorando nuestros sistemas, por favor inténtelo otro día";
            return Ok(new
            {
                Status = "Mantenimiento",
                Message = "Temporalmente se encuentra en mantenimiento. " + mensaje
            });
        }
    }
}