using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string ClientName { get; set; }
        public int UserId { get; set; }
    //    public string UserName { get; set; }
        public DateTime RegisteredDate { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
       // public bool IsPrinted { get; set; }
    //    public int PaymentMethodId { get; set; }
      //  public int ItemsCount { get; set; }
    }
}
