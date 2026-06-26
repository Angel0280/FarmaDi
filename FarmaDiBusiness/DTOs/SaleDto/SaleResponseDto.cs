using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SaleDto
{
    public class SaleResponseDto
    {
        public int InvoiceId { get; set; }
        public int UserId { get; set; }
        public string ClientName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPrinted { get; set; }
        public int PaymethMethodId { get; set; }
        public List<SalesDetailsResponseDto> Details { get; set; }



    }
}
