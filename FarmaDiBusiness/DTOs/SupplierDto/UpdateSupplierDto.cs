using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SupplierDto
{
    public class UpdateSupplierDto
    {

        [Required]
        public required string SupplierName { get; set; }

        // RNC ahora opcional (igual que en Add)
        public string? RNC { get; set; }

        [EmailAddress(ErrorMessage = "El campo Mail debe ser un correo electrónico válido.")]
        public string? Mail { get; set; }

        [Required(ErrorMessage = "El campo SupplierPhone es obligatorio.")]
        public required string SupplierPhone { get; set; }

        [Required(ErrorMessage = "El campo SupplierAddress es obligatorio.")]
        public required string SupplierAddress { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
