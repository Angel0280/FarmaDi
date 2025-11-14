using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.Roles
{
    public class AddRolDto
    {
        [Required]
        [StringLength(100) ]
        public string RolName { get; set; }
        //public bool IsActive { get; set; }
    }
}
