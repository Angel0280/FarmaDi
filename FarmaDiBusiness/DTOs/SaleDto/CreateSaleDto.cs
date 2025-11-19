using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SaleDto
{
    public class CreateSaleDto
    {
        [Required(ErrorMessage = "El id del ususairo debe de ser obligatorio")]
        
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        public string ClientName { get; set; }

        public int PaymentMethodId { get; set; }

        public decimal Discount { get; set; }

        [Required(ErrorMessage = "Por lo menos debe de haber un producto en la compra.")]
        
        public List<CreateSaleDetailDto> Details { get; set; }

    }
}
