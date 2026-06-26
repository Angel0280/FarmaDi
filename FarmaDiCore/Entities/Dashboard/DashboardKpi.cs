using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities.Dashboard
{
    public class DashboardKpi
    {
        public List<SalesByPeriod> SalesByPeriod { get; set; } = [];
        public List<TopProductRevenue> TopProductRevenues { get; set; } = [];
        public List<TopProductSold> TopProductsSold { get; set; } = [];
        public List<QuarterSales> QuarterSales { get; set; } = [];
        public List<MonthlyGrowth> MonthlyGrowth { get; set; } = [];
    }
}
