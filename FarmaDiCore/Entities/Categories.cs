using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Categories
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public bool IsActive { get; set; }
    }
}
