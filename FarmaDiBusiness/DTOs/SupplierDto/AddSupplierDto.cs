using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SupplierDto
{
    public class AddSupplierDto
    {
        [Required]
        public required string SupplierName { get; set; }
        public string? RNC { get; set; }

        [EmailAddress(ErrorMessage = "El campo Mail debe ser un correo electrónico válido.")]
        public string? Mail { get; set; }

        [Required(ErrorMessage = "El campo SupplierPhone es obligatorio.")]
        public required string SupplierPhone { get; set; }

        [Required(ErrorMessage = "El campo SupplierAddress es obligatorio.")]
        public required string SupplierAddress { get; set; }
    }
}
