using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.InventoryLossDto
{
    public class AddInventoryLossDto
    {
        [Required]
        public int BatchId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "La razón  de la baja  debe tener entre 5 y  500 caracteres")]
        public string Reason { get; set; }
    }
}
