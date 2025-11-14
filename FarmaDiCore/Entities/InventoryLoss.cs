using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class InventoryLoss
    {
      public int LowId { get; set; }

      public ProductBatches oBatch { get; set; }
      public int BatchId { get; set; }
      public string BatchNumber { get; set; }
      public int Quantity { get; set; }
      public Products oProduct { get; set; }
      public int ProductId {  get; set; }
      public string ProductGenericName { get; set; }
      public string ProductTradeName { get; set; }
      public Users oUser { get; set; }
      public int UserId {  get; set; }
      public string UserName { get; set; }
      public string Reason { get; set; }
    }
}
