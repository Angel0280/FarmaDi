using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.SaleDto
{
    public class SalesDetailsResponseDto
    {
        public int ProductId { get; set; }
        public string ProductTradeName { get; set; }   
        public string ProductGenericName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Total { get; set; }


    }
}
