using FarmaDiBusiness.DTOs.Roles;
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
    public interface IUsersService
    {
        Task<ServiceResponse<RolesUers>> RegisterUserWithRolesAsync(RegisterUserRolesDto userDto);

        //Task<ServiceResponse<Users>> GetUSerByNameAsync(string name);

    }
}
