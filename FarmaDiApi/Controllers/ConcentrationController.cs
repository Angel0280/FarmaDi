using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class concentrationsController : ControllerBase
    {
        private readonly IConcentrationServices _concentrationsService;

        public concentrationsController(IConcentrationServices concentrationsService)
        {
            _concentrationsService = concentrationsService;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _concentrationsService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var concentrationsDtoCollection = serviceResponse.Data!.Select(b => new ConcentrationDto
                {
                    ConcentrationId = b.ConcentrationId,
                    Volume = b.Volume,
                    porcentage = b.porcentage,
                    IsActive = b.IsActive
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<ConcentrationDto>>
                {
                    Data = concentrationsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = concentrationsDtoCollection.Count,
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

       
    }
}