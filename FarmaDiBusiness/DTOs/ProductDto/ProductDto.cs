using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.ProductDto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string GenericName { get; set; }
        public string TradeName { get; set; }
        public int CategoryId { get; set; }
       
        
        public int PresentationId { get; set; }
       
        public int ConcentrationId { get; set; }
      
        public int SupplierId { get; set; }
        public int BrandId { get; set; }
        
        public bool IsActive { get; set; }

    }
}
