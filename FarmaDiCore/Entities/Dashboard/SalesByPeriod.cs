using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities.Dashboard
{
    public class SalesByPeriod
    {
        public string Periodo { get; set; } = string.Empty;

        public decimal TotalVentas { get; set; }
    }
}
