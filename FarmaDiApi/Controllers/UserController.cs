using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsersService _userService;
        public UserController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserRolesDto dto)
        {
            //El framework ya se encarga de validar el formato del dto
            var serviceResponse = await _userService.RegisterUserWithRolesAsync(dto);
            if (serviceResponse.IsSuccess)
            {
                var dataResponse = new UserRolResponseDto();

                dataResponse.UserName = serviceResponse.Data.Users.UserName;
                dataResponse.UserLastName = serviceResponse.Data.Users.UserLastName;
                dataResponse.Mail = serviceResponse.Data.Users.Mail;
                dataResponse.UserPhone = serviceResponse.Data.Users.UserPhone;

                dataResponse.Roles = (IEnumerable<string>)serviceResponse.Data!.Roles.Select(dt => new RolesResponseDto
                {
                    RolId = dt.Id,
                    RolName = dt.RolName
                    

                }).ToList();

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = dataResponse.Roles },
                    dataResponse

                    );
            }

            var unSuccessFullResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessFullResponse.Code = "400";
                    unSuccessFullResponse.Message = "Ocurrio un error en la validacion de los datos";

                    return BadRequest(unSuccessFullResponse);

                default:
                    unSuccessFullResponse.Code = "500";
                    unSuccessFullResponse.Message = "Ocurrio un error en el servidor";
                    unSuccessFullResponse.Details = new { info = serviceResponse.Message};
                    return StatusCode(500, unSuccessFullResponse);

            }



        }

        [HttpGet]
        public async Task<IActionResult> GetById (int id)
        {
            return Ok();
        }
    }
}
