using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
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

        // CORRECCIÓN: El GetById necesita un parámetro {id} para que CreatedAtAction funcione.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Aquí iría la lógica para buscar una compra por ID
            // (Asumimos que GetByIdAsync(id) existe en el servicio)
            return Ok($"Obteniendo compra {id}");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreatePurchaseDto dto)
        {
            // 1. Llamar al servicio.
            // Nota: El servicio AHORA debe devolver ServiceResponse<PurchaseResponseDto>
            var serviceResponse = await _purchaseService.InsertAsync(dto);

            // 2. Verificar si fue exitoso
            if (serviceResponse.IsSuccess)
            {
                // 3. Devolver el DTO de respuesta
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data!.Id }, // El ID viene del DTO
                    serviceResponse.Data
                );
            }

            // 4. LÓGICA DE ERROR (Ahora sí se ejecuta)
            var UnSuccessFulresponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    UnSuccessFulresponse.Code = "400";
                    UnSuccessFulresponse.Message = "Ocurrió un error en la validación de datos";
                    UnSuccessFulresponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(UnSuccessFulresponse);

                // Puedes agregar más casos (ej. Conflict, NotFound)

                default: // ErrorDataBase o Inesperado
                    UnSuccessFulresponse.Code = "500";
                    UnSuccessFulresponse.Message = "Ocurrió un error inesperado";
                    UnSuccessFulresponse.Details = new { info = serviceResponse.Message };
                    // Es mejor usar 500 para errores de servidor
                    return StatusCode(500, UnSuccessFulresponse);
            }
        }
    }
}