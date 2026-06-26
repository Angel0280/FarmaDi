using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class SaleTransaction
    {
        public Sale Sale { get; set; }
        public Invoice InvoiceMaster { get; set; }
        public IEnumerable<SaleDetails> SaleDetailsList { get; set; }
        public List<InvoiceDetails> InvoiceDetails { get; set; }
    }
}
