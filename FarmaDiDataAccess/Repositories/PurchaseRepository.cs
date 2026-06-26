using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly string _connectionString; // CORREGIDO: Estándar camelCase privado
        private const string StoredProcedureName = "USP_InsertPurchase";
        private const string UdttTypeName = "PurchaseDetailsType";

        public PurchaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PurchaseTransaction>> InserAsync(Purchase master, IEnumerable<PurchaseDetails> details)
        {
            var purchaseTransaction = new PurchaseTransaction(); // CORREGIDO: Renombrado para evitar confusión con SqlTransaction
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand(StoredProcedureName, connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Manejo seguro de la fecha de la orden de compra
                    DateTime purchaseDate = master.RegisteredDate == default ? DateTime.Now : master.RegisteredDate;

                    // 2. Vinculación de Parámetros Maestros
                    cmd.Parameters.AddWithValue("@SupplierId", master.SupplierId);
                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate);
                    cmd.Parameters.AddWithValue("@Observations", (object)master.Observation ?? DBNull.Value);

                    // 3. Crear el DataTable con la estructura EXACTA del UDTT en SQL Server
                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));
                    detailsTable.Columns.Add("UnitPrice", typeof(decimal));
                    detailsTable.Columns.Add("BatchNumber", typeof(string));
                    detailsTable.Columns.Add("ManufacturingDate", typeof(DateTime));
                    detailsTable.Columns.Add("ExpirationDate", typeof(DateTime));

                    // 4. Llenar el DataTable controlando valores vacíos o por defecto de forma segura
                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice,
                            item.BatchNumber ?? (object)DBNull.Value,
                            item.ManufacturingDate == default || item.ManufacturingDate == null ? DBNull.Value : item.ManufacturingDate,
                            item.ExpirationDate
                        );
                    }

                    // 5. Configuración del parámetro estructurado (UDTT)
                    SqlParameter detailParm = cmd.Parameters.AddWithValue("@PurchaseDetails", detailsTable);
                    detailParm.SqlDbType = SqlDbType.Structured;
                    detailParm.TypeName = UdttTypeName;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // LECTURA DEL MAESTRO (PRIMER RESULT SET)
                        if (await reader.ReadAsync())
                        {
                            purchaseTransaction.Master = new Purchase
                            {
                                PurchaseId = (int)reader["PurchaseId"],
                                SupplierId = (int)reader["SupplierId"],
                                UserId = (int)reader["UserId"],
                                Total = (decimal)reader["Total"],
                                Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                RegisteredDate = (DateTime)reader["RegisteredDate"],
                                PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString()
                            };
                        }

                        // Pasar al segundo Result Set (Detalle de la compra)
                        await reader.NextResultAsync();

                        // LECTURA DEL DETALLE (SEGUNDO RESULT SET)
                        var detailsList = new List<PurchaseDetails>();
                        while (await reader.ReadAsync())
                        {
                            detailsList.Add(new PurchaseDetails
                            {
                                Id = (int)reader["PurchaseDetailId"], // CORREGIDO: Mapeo exacto de la llave del detalle
                                PurchaseId = (int)reader["PurchaseId"],
                                ProductId = (int)reader["ProductId"],
                                BatchId = (int)reader["BatchId"],
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"],
                                TotalPrice = (decimal)reader["TotalPrice"],
                                RegisteredDate = (DateTime)reader["RegisteredDate"],
                                BatchNumber = reader["BatchNumber"] is DBNull ? null : reader["BatchNumber"].ToString(),
                                ManufacturingDate = reader["ManufacturingDate"] is DBNull ? null : (DateTime?)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"]
                            });
                        }

                        purchaseTransaction.Details = detailsList;
                    }

                    return new RepositoryResponse<PurchaseTransaction>
                    {
                        Data = purchaseTransaction,
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = $"Error de Base de Datos ({ex.Number}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = $"Error General en Infraestructura de Compras: {ex.Message}"
                };
            }
        }
    }
}