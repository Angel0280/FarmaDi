using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class CreatePurchaseDetailsDto
    {

        [Required(ErrorMessage = "El id del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del producto no es válido")]
        public int ProductId { get; set; }

        // ¡CAMBIOS CLAVE PARA LOTES!
        // 1. BatchId se ELIMINA de la entrada, ya que se genera en la BD.

        [Required(ErrorMessage = "El número de lote es requerido")]
        [StringLength(50, ErrorMessage = "El número de lote no puede exceder 50 caracteres")]
        public string BatchNumber { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime ExpirationDate { get; set; }

        // La fecha de fabricación puede ser opcional (nullable)
        public DateTime? ManufacturingDate { get; set; }
        // -------------------------

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad no es válida")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El precio unitario no es válido")]
        public decimal UnitPrice { get; set; }

    }
}
