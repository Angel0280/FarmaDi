using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities.Dashboard
{
    public class MonthlyGrowth
    {
        public string Periodo { get; set; } = string.Empty;

        public decimal TotalVentas { get; set; }

        public decimal? VentasMesAnterior { get; set; }

        public decimal? PorcentajeCrecimiento { get; set; }
    }
}
