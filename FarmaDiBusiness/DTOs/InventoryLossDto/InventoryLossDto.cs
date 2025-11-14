using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.InventoryLossDto
{
    public class InventoryLossDto
    {
        public int LowId { get; set; }
        public int BatchId { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
  
        public int UserId { get; set; }
  
        public string Reason { get; set; }
    }
}
