using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class PurchaseTransaction
    {
        public Purchase Master {  get; set; }
        public List<PurchaseDetails> Details { get; set; }
    }
}
