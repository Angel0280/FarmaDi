using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class UserRolResponseDto
    {
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string Mail { get; set; }
        public string UserPhone { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
