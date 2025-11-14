using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Presentations
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Quantity { get; set; }
        public string? UnitMeasure { get; set; }
        public bool IsActive { get; set; }
    }
}
