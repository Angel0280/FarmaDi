using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class SaleDetails
    {
        public int SalesDetailId { get; set; }
        public int SaleId { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime RegisteredDate { get; set; }


    }
}
