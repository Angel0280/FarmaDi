using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IAuthService
    {
        Task <ServiceResponse<Users>> RegisterAsync (AddUserDto newuser);
        Task <ServiceResponse<Users>> GetByEmailAsync (string mail);
        Task <ServiceResponse<Users>> GetByNameAsync (string name);
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto);
    }
}
