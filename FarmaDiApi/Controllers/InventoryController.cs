using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _inventoryService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de los datos recibidos a la estructura del GetAllInventoryDto usando LINQ
                var inventoryDtoCollection = serviceResponse.Data!.Select(i => new GetAllInventoryDto
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.oproduct.ProductId,
                    ProductGenericname = i.oproduct.GenericName,
                    SalePrice = i.SalePrice,
                    PurchasePrice = i.PurchasePrice,
                    CriticalStock = i.CriticalStock
                }).ToList(); // CORREGIDO: .ToList() evita doble enumeración al contar y responder

                var apiResponse = new ApiResponse<IEnumerable<GetAllInventoryDto>>
                {
                    Data = inventoryDtoCollection,
                    Meta = new
                    {
                        TotalAmount = inventoryDtoCollection.Count,
                        message = serviceResponse.Message
                    }
                };
                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) // CORREGIDO: Removido 'id == null' por ser un int primitivo
            {
                var response = new UnsuccessfulResponseDto
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _inventoryService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                // CORREGIDO: Nombre de variable homogeneizado (Antes se llamaba brandDto)
                var inventoryDto = new GetAllInventoryDto
                {
                    InventoryId = serviceResponse.Data!.InventoryId,
                    ProductId = serviceResponse.Data.oproduct.ProductId,
                    ProductGenericname = serviceResponse.Data.oproduct.GenericName,
                    SalePrice = serviceResponse.Data.SalePrice,
                    PurchasePrice = serviceResponse.Data.PurchasePrice,
                    CriticalStock = serviceResponse.Data.CriticalStock,
                };

                return Ok(inventoryDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = $"No se encontró ningun dato relacionado al Id {id}";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no esperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }


        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
    [FromQuery] int? categoryId,
    [FromQuery] string? estado,
    [FromQuery] int? brandId,
    [FromQuery] int? supplierId,
    [FromQuery] DateTime? fechaCorte,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10)
        {
            // 1. Validaciones de la paginación
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 10;
            if (limit > 100) limit = 100;

            // 2. Llamada al servicio
            var serviceResponse = await _inventoryService.GetDashboardAsync(page, limit, categoryId, estado, brandId, supplierId, fechaCorte);

            // 3. Manejo de éxito
            if (serviceResponse.IsSuccess)
            {
                // Tu DTO completo devuelto por el servicio
                var dashboardData = serviceResponse.Data;

                // Extraemos el total de registros directamente desde el Summary de tu DTO
                int totalCount = dashboardData.Summary.TotalProductos;

                // Calculamos las páginas
                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                // Preparamos el ApiResponse combinando los datos y la metadata
                var apiResponse = new ApiResponse<InventoryDashboardDto>
                {
                    Data = dashboardData,
                    Meta = new GetPagedDto
                    {
                        TotalItems = totalCount,
                        TotalPages = totalPages,
                        CurrentPage = page,
                        ItemsPerPage = limit
                    }
                };

                return Ok(apiResponse);
            }

            // 4. Manejo de errores
            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros de inventario";
                    unsuccessfulResponse.Details = new { info = "No hay datos que coincidan con los filtros proporcionados" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado al cargar el dashboard";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}