using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly string _ConnectionString;
        private const string StoredProcedureName = "USP_InsertPurchase";
        private const string UdttTypeName = "PurchaseDetailsType";

        public PurchaseRepository(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PurchaseTransaction>> InserAsync(Purchase master, IEnumerable<PurchaseDetails> details)
        {
            var transaction = new PurchaseTransaction();
            try
            {
                using (SqlConnection connection = new SqlConnection(_ConnectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand(StoredProcedureName, connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // 1. Asumo que las fechas deben ser pasadas explícitamente al SP.
                    //    Si no existe master.PurchaseDate, usa DateTime.Now o pídela como parámetro.
                    //    ¡Tu SP necesita @PurchaseDate!
                    DateTime purchaseDate = master.RegisteredDate == default ? DateTime.Now : master.RegisteredDate;

                    // 2. Parámetros Maestros
                    cmd.Parameters.AddWithValue("@SupplierId", master.SupplierId);
                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate); // <-- AGREGADO
                    // Se usa "TotalAmount" o "Total" en la tabla, pero el SP usa un cálculo interno.
                    // El SP usa @Observations, no @Observation
                    cmd.Parameters.AddWithValue("@Observations", (object)master.Observation ?? DBNull.Value);
                   // cmd.Parameters.AddWithValue("@PurchaseNum", (object)master.PurchaseNum ?? DBNull.Value); // <-- AGREGADO (si aplica)


                    // 3. Crear el DataTable con la estructura EXACTA del UDTT en SQL Server
                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));          // Cantidad debe ser INT o DECIMAL según UDTT
                    detailsTable.Columns.Add("UnitPrice", typeof(decimal));
                    detailsTable.Columns.Add("BatchNumber", typeof(string));    // <-- AGREGADO
                    detailsTable.Columns.Add("ManufacturingDate", typeof(DateTime)); // <-- AGREGADO
                    detailsTable.Columns.Add("ExpirationDate", typeof(DateTime));    // <-- AGREGADO

                    // 4. Llenar el DataTable
                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice,
                            item.BatchNumber,           // Asumiendo que ahora tu entidad tiene BatchNumber
                            (object)item.ManufacturingDate ?? DBNull.Value,
                            item.ExpirationDate
                        );
                    }

                    // 5. Corregir la configuración del parámetro UDTT
                    // ERROR 1: Usaste 'details' (la lista C#) en lugar de 'detailsTable' (el DataTable)
                    // ERROR 2: El parámetro en el SP se llama @PurchaseDetails, no @PurchaseDetail
                    SqlParameter detailParm = cmd.Parameters.AddWithValue("@PurchaseDetails", detailsTable); // <-- CORREGIDO

                    detailParm.SqlDbType = SqlDbType.Structured;
                    detailParm.TypeName = UdttTypeName;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // LECTURA DEL MAESTRO (PRIMER RESULT SET)
                        if (await reader.ReadAsync())
                        {
                            transaction.Master = new Purchase
                            {
                                // ************ Posible ERROR en el casting de PurchaseNum (Puede ser String/Int) ***********
                                PurchaseId = (int)reader["PurchaseId"],
                                SupplierId = (int)reader["SupplierId"],
                                UserId = (int)reader["UserId"],
                                Total = (decimal)reader["Total"],
                                Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                RegisteredDate = (DateTime)reader["RegisteredDate"],
                                // Se asume que PurchaseNum es String/NVARCHAR para evitar errores de casting.
                                PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString()
                            };
                        }

                        // Pasar al segundo Result Set (Detalle)
                        await reader.NextResultAsync();


                        // LECTURA DEL DETALLE (SEGUNDO RESULT SET)
                        var detailsList = new List<PurchaseDetails>();
                        while (await reader.ReadAsync())
                        {
                            detailsList.Add(new PurchaseDetails
                            {
                                // ************ El campo es PurchaseDetailId, no Id ***********
                                Id = (int)reader["PurchaseDetailId"],
                                PurchaseId = (int)reader["PurchaseId"],
                                ProductId = (int)reader["ProductId"],
                                BatchId = (int)reader["BatchId"],
                                // Quantity se lee como INT o DECIMAL. Lo dejé como INT.
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"],
                                TotalPrice = (decimal)reader["TotalPrice"],
                                RegisteredDate = (DateTime)reader["RegisteredDate"],


                                BatchNumber = reader["BatchNumber"].ToString(),
                                ManufacturingDate = reader["ManufacturingDate"] is DBNull ? null : (DateTime?)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"]
                            });
                        }

                        transaction.Details = detailsList;
                    }

                    return new RepositoryResponse<PurchaseTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            // Mueve la excepción más genérica al final para capturar primero las específicas (SqlException)
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
                    Message = $"Error General: {ex.Message}",
                };
            }
        }
    }
}