using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs
{
    public class AddBrandDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de la marca debe tener entre 3 y  100 caracteres")]
        public string BrandName { get; set; }
        [StringLength(500, ErrorMessage = "La descripción no debe exceder los  500 caracteres")]
        public string BrandDescription { get; set; }


    }
}
