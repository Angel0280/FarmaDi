using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class UpdateUserDto
    {
        [Required]
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,100}$", ErrorMessage =
            "La contraseña debe contener mayúsculas, minúsculas, números y símbolos.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "El campo del email no debe de ir vacio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Mail { get; set; }
        [Phone]
        [Required]
        [StringLength(8, ErrorMessage = "El número de teléfono debe tener 8 dígitos.")]
        public string UserPhone { get; set; }
    }
}
