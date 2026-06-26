using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) // CORREGIDO: Protección perimetral de entrada para consistencia REST
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            // NOTA: Cuando implementes la lectura, este stub llamará de forma segura a tu servicio.
            return Ok(new { Message = $"Obteniendo compra {id}" });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreatePurchaseDto dto)
        {
            var serviceResponse = await _purchaseService.InsertAsync(dto);

            if (serviceResponse.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data!.Id },
                    serviceResponse.Data);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto(); // CORREGIDO: Casing camelCase
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound: // NUEVO: Intercepta fallos si el producto o proveedor no existen en la BD
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso transaccional no encontrado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Uno o varios IDs relacionales no existen en el sistema" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.Conflict: // NUEVO: Intercepta conflictos de negocio o de concurrencia
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "Conflicto al procesar la transacción de compra";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se pudo completar la operación por un conflicto de datos" };
                    return Conflict(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Ocurrió un error en la validación de datos";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(unsuccessfulResponse);

                default: // ErrorDataBase o excepciones de infraestructura imprevistas
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado en el servidor";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no controlado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}