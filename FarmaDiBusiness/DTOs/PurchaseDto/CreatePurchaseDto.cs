using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class CreatePurchaseDto
    {
        [Required(ErrorMessage = "El id del proveedor es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del proveedor no es válido")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "El id del Usuario es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del usuario no es válido")]
        public int UserId { get; set; }

        // Nuevo campo: Número de Compra/Factura
      //  [StringLength(50, ErrorMessage = "El número de compra no puede exceder 50 caracteres")]
      //  public string? PurchaseNum { get; set; }

        // Observation puede ser opcional
        [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres")]
        public string? Observation { get; set; }

        [Required(ErrorMessage = "La compra debe incluir un detalle de compra")]
        [MinLength(1, ErrorMessage = "El detalle debe incluir al menos un producto")]
        public List<CreatePurchaseDetailsDto> Details { get; set; }
    }
}
