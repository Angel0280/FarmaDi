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
        public  string Volume { get; set; }
        public string? porcentage { get; set; }
        public bool IsActive { get; set; }

    }
}