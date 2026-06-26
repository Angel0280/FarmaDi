using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<Inventory>>> GetAllAsync()
        {
            var inventoryList = new List<Inventory>(); // CORREGIDO: Nombre semántico para evitar confusión visual
            var response = new RepositoryResponse<IEnumerable<Inventory>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductPricing", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            inventoryList.Add(new Inventory
                            {
                                InventoryId = (int)reader["Id"],
                                oproduct = new Products
                                {
                                    ProductId = (int)reader["ProductId"],
                                    GenericName = reader["ProductGenericName"].ToString()!
                                },
                                SalePrice = (decimal)reader["SalePrice"],
                                PurchasePrice = (decimal)reader["PurchasePrice"],
                                CriticalStock = (int)reader["CriticalStock"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = inventoryList;
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message; // CORREGIDO: Añadida trazabilidad del mensaje de error nativo
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Inventory>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Inventory>> GetByIdAsync(int id)
        {
            var inventoryResult = new Inventory(); // CORREGIDO: Renombrado para diferenciarlo del wrapper de retorno
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductPricingById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            inventoryResult.InventoryId = (int)reader["Id"];
                            inventoryResult.oproduct = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!
                            };
                            inventoryResult.SalePrice = (decimal)reader["SalePrice"];
                            inventoryResult.PurchasePrice = (decimal)reader["PurchasePrice"];
                            inventoryResult.CriticalStock = (int)reader["CriticalStock"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Inventory>
                    {
                        Data = inventoryResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Agregado bloque catch genérico para mitigar crashes inesperados en producción
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }



        public async Task<RepositoryResponse<InventoryDashboard>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte)
        {
            var response = new RepositoryResponse<InventoryDashboard>
            {
                Data = new InventoryDashboard()
            };

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventory", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametros de entrada para la paginacion
                    cmd.Parameters.AddWithValue("@PageNumber", page);
                    cmd.Parameters.AddWithValue("@PageSize", limit);

                    // 🔴 SOLUCIÓN: Declaración del parámetro de salida (Output)
                    SqlParameter totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(totalRecordsParam);

                    // Parámetros opcionales (Manejo de NULL para SQL)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(estado) ? DBNull.Value : estado);
                    cmd.Parameters.AddWithValue("@BrandId", brandId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId", supplierId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaCorte", fechaCorte ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // RESULT SET 1: Resumen (Tarjetas)
                        if (await reader.ReadAsync())
                        {
                            response.Data.Summary.TotalProductos = reader["TotalProductos"] != DBNull.Value ? Convert.ToInt32(reader["TotalProductos"]) : 0;
                            response.Data.Summary.StockBajo = reader["StockBajo"] != DBNull.Value ? Convert.ToInt32(reader["StockBajo"]) : 0;
                            response.Data.Summary.Agotados = reader["Agotados"] != DBNull.Value ? Convert.ToInt32(reader["Agotados"]) : 0;
                            response.Data.Summary.ValorInventario = reader["ValorInventario"] != DBNull.Value ? Convert.ToDecimal(reader["ValorInventario"]) : 0;
                        }

                        // RESULT SET 2: Lista Principal
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Data.Items.Add(new InventoryItem
                                {
                                    ProductId = (int)reader["ProductId"],
                                    Producto = reader["Producto"].ToString(),
                                    NombreGenerico = reader["NombreGenerico"].ToString(),
                                    CategoryId = (int)reader["CategoryId"],
                                    Categoria = reader["Categoria"].ToString(),

                                    // Manejo de nulos para IDs opcionales
                                    PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                    ConcentrationId = reader["ConcentrationId"] != DBNull.Value ? (int)reader["ConcentrationId"] : null,
                                    ConcentrationValue = reader["ConcentrationValue"] != DBNull.Value ? reader["ConcentrationValue"].ToString() : string.Empty,
                                    SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : null,
                                    BrandId = reader["BrandId"] != DBNull.Value ? (int)reader["BrandId"] : null,

                                    Isactive = (bool)reader["Isactive"],
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                                    PrecioCosto = reader["PrecioCosto"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioCosto"]) : 0,
                                    StockCritico = reader["StockCritico"] != DBNull.Value ? Convert.ToInt32(reader["StockCritico"]) : 0,

                                    Existencia = Convert.ToInt32(reader["Existencia"]),
                                    CantidadVencida = Convert.ToInt32(reader["CantidadVencida"]),
                                    ValorProducto = Convert.ToDecimal(reader["ValorProducto"]),
                                    Estado = reader["Estado"].ToString()
                                });
                            }
                        }

                        // RESULT SET 3: Detalle de Lotes
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Data.Batches.Add(new InventoryBatchInfo
                                {
                                    BatchId = (int)reader["BatchId"],
                                    NumeroLote = reader["NumeroLote"].ToString(),
                                    FechaFabricacion = Convert.ToDateTime(reader["FechaFabricacion"]),
                                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                                    CantidadOriginal = Convert.ToInt32(reader["CantidadOriginal"]),
                                    CantidadDisponible = Convert.ToInt32(reader["CantidadDisponible"]),
                                    ProductId = (int)reader["ProductId"],
                                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                                    Activo = (bool)reader["Activo"],
                                    StockId = (int)reader["StockId"],
                                    FechaEntradaStock = Convert.ToDateTime(reader["FechaEntradaStock"]),
                                    EstadoLote = reader["EstadoLote"].ToString()
                                });
                            }
                        }
                    } // IMPORTANTE: El DataReader se cierra aquí.

                    // Asignamos 0 como éxito porque este SP no retorna un @ReturnValue explícito
                    response.OperationStatusCode = 0;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}