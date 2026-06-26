using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SaleDto
{
    public class CreateSaleDetailDto
    {
        [Required(ErrorMessage = "El id del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del producto es invalido")]
        public int ProductId { get; set; } 

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad no es válida")]
        public int Quantity { get; set; }
        public decimal UnitPrice { get; internal set; }
    }
}
