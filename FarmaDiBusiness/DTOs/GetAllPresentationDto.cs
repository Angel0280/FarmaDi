using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs
{
    public class GetAllPresentationDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string UnitMeasure { get; set; }
        public bool IsActive { get; set; }
    }
}
