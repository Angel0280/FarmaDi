using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class PurchaseResponseDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int UserId { get; set; }      
        public decimal total { get; set; }
        public string? Observation { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseNum { get; set; }
        public List<PurchaseDetailsResponseDto> Details { get; set; }
    }
}
