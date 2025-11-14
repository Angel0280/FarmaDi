using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Concentrations
    {
        public  int ConcentrationId { get; set; }
        public  string ConcentrationName { get; set; }
        public string? ConcentrationDescription { get; set; }
        public bool IsActive { get; set; }

    }
}