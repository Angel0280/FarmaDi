using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class InvoiceDetails
    {
        public int InvoicesDetailId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string? ProductTradeName { get; set; }
        public string? ProductGenericName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
