using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

  

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateSaleDto dto)
        {
            var serviceResponse = await _saleService.InsertAsync(dto);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Se inyecta serviceResponse.Data como tercer parámetro para retornar el JSON creado
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data!.InvoiceId },
                    serviceResponse.Data
                );
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto(); // CORREGIDO: Corrección de typo y casing

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound: // NUEVO: Intercepta de forma limpia si el usuario o producto no existen
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso no encontrado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "El usuario o el producto especificado no existen." };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.Conflict: // NUEVO: Intercepta semánticamente la falta de Stock en inventario (HTTP 409)
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "Conflicto al procesar la venta";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No hay suficiente stock disponible para completar la transacción." };
                    return Conflict(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Ocurrió un error de validación de datos.";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(unsuccessfulResponse);

                default: // ErrorDataBase o excepciones imprevistas del servidor (HTTP 500)
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió algo inesperado en el servidor.";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no controlado." };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("{id:int}")] // ✅ Restricción de ruta: solo enteros positivos
        public async Task<IActionResult> GetById(int id)
        {
            // 1. Validación de formato de entrada (HTTP)
            if (id <= 0)
            {
                var badRequestResponse = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Solicitud inválida",
                    Details = new { info = "El ID de factura debe ser un número entero positivo." }
                };
                return BadRequest(badRequestResponse);
            }

            // 2. Invocación al servicio de negocio
            var serviceResponse = await _saleService.GetByIdAsync(id);

            // 3. Procesamiento de la respuesta del servicio
            if (serviceResponse.IsSuccess && serviceResponse.Data is not null)
            {
                return Ok(serviceResponse.Data);
            }

            // 4. Mapeo de errores de negocio a respuestas HTTP semánticas
            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso no encontrado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "La factura solicitada no existe." };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.BadRequest:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Solicitud inválida";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "El identificador proporcionado no es válido." };
                    return BadRequest(unsuccessfulResponse);

                default: // ErrorDataBase o excepciones imprevistas del servidor (HTTP 500)
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió algo inesperado en el servidor.";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no controlado al recuperar la factura." };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetSales(
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10)
        {
            // 1. Validación rápida de parámetros de entrada
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Solicitud inválida",
                    Details = new { info = "Los parámetros 'pageNumber' y 'pageSize' deben ser enteros positivos mayores a cero." }
                });
            }

            // 2. Invocación al servicio
            var serviceResponse = await _saleService.GetSalesPagedAsync(pageNumber, pageSize);

            // 3. RESPUESTA DE ÉXITO: Si es correcto, retornamos Ok y cortamos la ejecución con 'return'
            if (serviceResponse.IsSuccess && serviceResponse.Data is not null)
            {
                return Ok(serviceResponse.Data);
            }

            // =========================================================================
            // Manejo de errores manual (Solo se ejecuta si la condición de arriba falla)
            // =========================================================================
            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "Recurso no encontrado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontraron facturas." };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.BadRequest:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Solicitud inválida";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Los datos proporcionados no son válidos." };
                    return BadRequest(unsuccessfulResponse);

                default: // En caso de que falle la base de datos o salte una excepción real
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió algo inesperado en el servidor.";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no controlado al recuperar las ventas." };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}