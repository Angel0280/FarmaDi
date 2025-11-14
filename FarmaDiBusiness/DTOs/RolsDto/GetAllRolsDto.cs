using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.RolsDto
{
    public class GetAllRolsDto
    {
        public int Id { get; set; }
        public string RolName { get; set; }
        public bool IsActive { get; set; }
    }
}
