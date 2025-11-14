using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class ProductBatches
    {
        public int Id { get; set; }
        public string BatchNumer {  get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpirationDate { get; set; }    
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Products oProduct { get; set; } // objeto de producto, se puede usar para navegar a cualquier otro atributo de producto
        public bool IsActive { get; set; }

    }
}
