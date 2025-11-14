using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryLossController : ControllerBase
    {

        private readonly IInventoryLossService _InventoryLossService;


        public InventoryLossController(IInventoryLossService inventoryLoss)
        {
            _InventoryLossService = inventoryLoss;
        }




        // GET: api/<InventoryLossController>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _InventoryLossService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {

                var InventoryLossDtoCollection = serviceResponse.Data!.Select(c => new GetAllInventoryLossDto
                {
                    LowId = c.LowId,
                    BatchId = c.oBatch.Id,
                    BatchNumber = c.oBatch.BatchNumer,
                    Quantity = c.Quantity,
                    ProductId = c.oProduct.ProductId,
                    ProductGenericName = c.oProduct.GenericName,
                    ProductTradeName = c.oProduct.TradeName,
                    UserId = c.UserId,
                    UserName = c.UserName,
                    Reason = c.Reason,
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllInventoryLossDto>>
                {
                    Data = InventoryLossDtoCollection,
                    Meta = new
                    {
                        TotalAmount = InventoryLossDtoCollection.Count(),
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

        // GET api/<InventoryLossController>/5
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
            var serviceResponse = await _InventoryLossService.GetByIdAsync(id);

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
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró una baja asociada al Id proporcionado",
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

        // POST api/<InventoryLossController>

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] AddInventoryLossDto addInventoryLossDto)
        {
            //primero se tiene que validar que los id proporcionados esten realacionados a algun registro de la db

            if (addInventoryLossDto.Quantity <= 0)
            {
                var unsuccessfulResponse = new UnsuccessfulResponseDto()
                {
                    Code = "409",
                    Message = "Cantidad inferior a la requerida",
                    Details = new { info = "La cantidad requerida para la operación debe ser mayor a 0" }
                };

                return Conflict(unsuccessfulResponse);

            }

            var id = addInventoryLossDto.BatchId;

           



            var serviceResponse = await _InventoryLossService.AddAsync(addInventoryLossDto);


            if (serviceResponse.IsSuccess)
            {
                //ncuando se insertan valores de los id que no estan la respuesta devuelve nulo aqui 
                var InvetoryLossDto = new InventoryLossDto
                {
                    LowId = serviceResponse.Data!.LowId,
                    BatchId = serviceResponse.Data!.oBatch.Id, 
                    Quantity = -serviceResponse!.Data.Quantity,
                    ProductId = serviceResponse!.Data.oProduct.ProductId,
                    UserId = serviceResponse!.Data.UserId,
                    Reason = serviceResponse!.Data.Reason,

                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = InvetoryLossDto.LowId },
                    InvetoryLossDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la  baja ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar información de la baja" };

                    return Conflict(unSuccessfulResponse);

                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "Recurso no encontrado";
                    unSuccessfulResponse.Details = new { info = "No existe un lote que corresponda al id brindado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return BadRequest(unSuccessfulResponse);



            }
        }

        // PUT api/<InventoryLossController>/5
        [HttpPut("{id}")]
        public string Put(int id, [FromBody] string value)
        {
            var mensaje = "Estamos mejorando nuestros sistemas, por favor inténtelo otro dia";
            return "Temporalmente se encuentra en mantenimiento. " + mensaje;
        }

  
    }
}
