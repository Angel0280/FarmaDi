using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class GetUsers
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string Mail { get; set; }
        public string UserPhone { get; set; }
        public int RolId { get; set; }
        public bool IsActive { get; set; }

        public List<string> Roles { get; set; }

    }
}
