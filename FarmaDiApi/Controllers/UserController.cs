using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
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

                dataResponse.Roles = serviceResponse.Data!.Roles.Select(dt => new RolesResponseDto
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
                    unSuccessFullResponse.Details = new { info = serviceResponse.Message };
                    return StatusCode(500, unSuccessFullResponse);

            }



        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var serviceResponse = await _userService.GetByIdAsync(id);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }
            var unSuccessFullResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessFullResponse.Code = "404";
                    unSuccessFullResponse.Message = "Usuario no encontrado";
                    return NotFound(unSuccessFullResponse);
                default:
                    unSuccessFullResponse.Code = "500";
                    unSuccessFullResponse.Message = "Ocurrio un error en el servidor";
                    return StatusCode(500, unSuccessFullResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var serviceResponse = await _userService.GetAllAsync();
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }
            var unSuccessFullResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorDataBase:
                    unSuccessFullResponse.Code = "500";
                    unSuccessFullResponse.Message = "Ocurrio un error en la base de datos";
                    return StatusCode(500, unSuccessFullResponse);
                default:
                    unSuccessFullResponse.Code = "500";
                    unSuccessFullResponse.Message = "Ocurrio un error en el servidor";
                    return StatusCode(500, unSuccessFullResponse);
            }
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleAssignmentDto assignmentDto)
        {
           
            var result = await _userService.AssignRoleToUserAsync(assignmentDto.UserId, assignmentDto.RoleId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            if (result.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(result);
            }

            if (result.MessageCode == MessageCodes.Conflict)
            {
                return Conflict(result);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }
    }
    /*public async Task<IActionResult> AssignRolToUser([FromBody] AssignRolToUserDto dto)
    {
        var serviceResponse = await _userService.AssignRolToUserAsync(dto);
        if (serviceResponse.IsSuccess)
        {
            return Ok(serviceResponse.Data);
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
                return StatusCode(500, unSuccessFullResponse);
        }*/
} 



