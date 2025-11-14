using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.ProductBatchesDto
{
    public class ProductBatchDto
    {
        public int BatchId { get; set; }
        public string BatchNumer {  get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public bool IsActive  { get; set; }
    }
}
