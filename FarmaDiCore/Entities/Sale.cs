using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string ClientName { get; set; }
        public int UserId  { get; set; }
        public DateOnly RegisteredDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public int PaymentMethodId { get; set; }
        

    }
}
