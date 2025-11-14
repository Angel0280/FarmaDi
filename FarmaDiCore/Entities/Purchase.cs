using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        // Se recomienda que sea string? si puede ser nulo en la BD
        public string? Observation { get; set; }
        public DateTime RegisteredDate { get; set; }
        // Se recomienda que sea string si el SP lo trata como NVARCHAR/string
        public string? PurchaseNum { get; set; }

    }
}
