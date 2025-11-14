using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.ProductDto
{
    public class UpdateProductDto
    {

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre comercial del producto debe contener entre 3 a 100 caracteres ")]
        public string TradeName { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre génerico del producto debe contener entre 3 a 100 caracteres ")]
        public string GenericName { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int PresentationId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ConcentrationId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int SupplierId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int BrandId { get; set; }
        [Required]
        public bool IsActive { get; set; } 
    }
}
