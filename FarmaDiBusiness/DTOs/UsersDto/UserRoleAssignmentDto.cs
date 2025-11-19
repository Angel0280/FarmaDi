using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class UserRoleAssignmentDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El ID de rol es obligatorio.")]
        public int RoleId { get; set; }
    }
}
