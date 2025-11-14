using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.StockDto
{
    public class StockDto
    {
        public int StockId { get; set; }
        public int BachtId { get; set; }
        public string BatchNumber { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
