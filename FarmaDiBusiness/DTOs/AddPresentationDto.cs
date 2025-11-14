using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs
{
    public class AddPresentationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La descripción  de la presentación debe tener entre 3 y  100 caracteres")]
        public string Description { get; set; }
        [Required]
        public string Quantity { get; set; }
        [Required]
        public string UnitMeasure { get; set; }

        // revisar tipo de datos ya que en la db lo que es la cantidad y unidad es int 
         
    }
}
