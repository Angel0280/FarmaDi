using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.Inventory
{
    public class GetAllInventoryDto
    {
        public int InventoryId  { get; set; }
        public int ProductId { get; set; }
        public string ProductGenericname { get; set; }
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice    { get; set; }
        public int CriticalStock { get; set; }
    }
}
