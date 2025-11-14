using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "El nombre de la categoria es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de la categoria debe tener entre 3 y 100 caracteres")]
        public string Name { get; set; }
        [StringLength(500, ErrorMessage = "La descripción no debe exceder los 500 caracteres")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Debe especificar si la categia  está activa o no")]
        public bool IsActive { get; set; }
    }
}
