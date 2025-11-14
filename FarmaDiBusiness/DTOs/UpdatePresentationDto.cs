using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs
{
    public class UpdatePresentationDto
    {
        [Required(ErrorMessage = "La descripción de la presentación es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La descripción de la presentación debe tener entre 3 y 100 caracteres")]
        public string Description { get; set; }
        [Required]
        public string Quantity { get; set; }
        [Required]

        public string UnitMeasure { get; set; }
        [Required(ErrorMessage = "Debe especificar  el estado de la presentación")]
        public bool IsActive { get; set; }
    }
}
