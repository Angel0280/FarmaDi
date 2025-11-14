using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.ProductDto
{
    public class GetAllProductsDto
    {
   
       public int ProductId { get; set; }

        public string TradeName { get; set; } 
        public string GenericName { get; set; }
       
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
     
       
        public int PresentationId { get; set; }
        public string PresentationName { get; set; }
   
        public int ConcentrationId { get; set; }
       
        //public string Volume {  get; set; }
        public string Porcentage { get; set; }
      
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        public bool IsActive { get; set; }

    }
}
