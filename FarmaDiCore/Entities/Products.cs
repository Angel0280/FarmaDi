using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public  class Products
    {
        public int ProductId { get; set; }
        public string? GenericName { get; set; }
        public string? TradeName { get; set; }
        public int CategoryId { get; set; }
        public Categories oCategory { get; set; }  // es un objeto que usare para acceder a mas propiedades de categorias
        
        public int PresentationId { get; set; }
        public Presentations oPresentation { get; set; } // igual q categoria
        public int ConcentrationId { get; set; }
        public Concentrations oconcentration { get; set; }
        public int SupplierId { get; set; }
        public Suppliers oSupplier { get; set; }
        public int BrandId { get; set; }
        public Brands obrand { get; set; }

        public bool IsActive { get; set; }

        

    }
}
