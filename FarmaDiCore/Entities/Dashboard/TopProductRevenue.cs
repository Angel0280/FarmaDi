using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities.Dashboard
{
    public class TopProductRevenue
    {
        public string Producto { get; set; } = string.Empty;

        public decimal TotalIngresos { get; set; }
    }
}
