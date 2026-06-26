using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities.Dashboard
{
    public class QuarterSales
    {
        public string Trimestre { get; set; } = string.Empty;

        public decimal TotalVentas { get; set; }
    }
}
