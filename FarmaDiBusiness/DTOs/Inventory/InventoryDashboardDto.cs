using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.Inventory
{
    public class InventoryDashboardDto
    {
        // 1. Contenedor Principal

        public InventorySummaryDto Summary { get; set; } = new InventorySummaryDto();
        public List<InventoryItemDto> Items { get; set; } = new List<InventoryItemDto>();
        public List<InventoryBatchInfoDto> Batches { get; set; } = new List<InventoryBatchInfoDto>();


        // 2. DTO para las Tarjetas de Resumen (Result Set 1)
        public class InventorySummaryDto
        {
            public int TotalProductos { get; set; }
            public int StockBajo { get; set; }
            public int Agotados { get; set; }
            public decimal ValorInventario { get; set; }
        }

        // 3. DTO para la Lista Principal de Inventario (Result Set 2)
        public class InventoryItemDto
        {
            public int ProductId { get; set; }
            public string Producto { get; set; }
            public string NombreGenerico { get; set; }
            public int CategoryId { get; set; }
            public string Categoria { get; set; }
            public int? PresentationId { get; set; }
            public int? ConcentrationId { get; set; }
            public string ConcentrationValue { get; set; }
            public int? SupplierId { get; set; }
            public int? BrandId { get; set; }
            public bool Isactive { get; set; }

            public decimal Precio { get; set; }
            public decimal PrecioCosto { get; set; }
            public int StockCritico { get; set; }

            public int Existencia { get; set; }
            public int CantidadVencida { get; set; }
            public decimal ValorProducto { get; set; }
            public string Estado { get; set; } // 'Normal', 'Critico', 'Agotado'
        }

        // 4. DTO para el Detalle de Lotes (Result Set 3)
        public class InventoryBatchInfoDto
        {
            public int BatchId { get; set; }
            public string NumeroLote { get; set; }
            public DateTime FechaFabricacion { get; set; }
            public DateTime FechaVencimiento { get; set; }
            public int CantidadOriginal { get; set; }
            public int CantidadDisponible { get; set; }
            public int ProductId { get; set; }
            public DateTime FechaRegistro { get; set; }
            public bool Activo { get; set; }

            public int StockId { get; set; }
            public DateTime FechaEntradaStock { get; set; }
            public string EstadoLote { get; set; } // 'Vencido', 'Por vencer', 'Agotado', 'Vigente'
        }
    }
}