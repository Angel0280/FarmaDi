using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Stock
    {
        public int Id { get; set; }
        public ProductBatches BatchId { get; set; }
        public int ProductId {  get; set; }
        public int AvailableQuantity { get; set; }

    }
}
