using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateSaleDto dto)
        {
            //Las validaciones del formato las hace el framework
            var serviceResponse = await _saleService.InsertAsync(dto);
            if (serviceResponse.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data!.InvoiceId}
                    );
            }

            var unSuccesfulResponse = new UnsuccessfulResponseDto();


            switch (serviceResponse.MessageCode)
            {
                case FarmaDiCore.Common.MessageCodes.ErrorValidation:
                    unSuccesfulResponse.Code = "400";
                    unSuccesfulResponse.Message = "Ocurrio un error de validacion de datos.";
                    unSuccesfulResponse.Details = new { info = serviceResponse.Message};

                    return BadRequest(unSuccesfulResponse);
                
                default:
                    unSuccesfulResponse.Code = "500";
                    unSuccesfulResponse.Message = "Ocurrio algo inesperado.";
                    unSuccesfulResponse.Details = new { info = serviceResponse.Message};

                    return StatusCode(500, unSuccesfulResponse);
            }
        }




    }
}
