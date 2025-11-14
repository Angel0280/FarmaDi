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
    public class SalesRepository : ISalesRepository
    {
        private readonly string _connectionString;
        public SalesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetails> details)
        {
            var trasaction = new SaleTransaction();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_InsertSale", connection);
                    cmd.CommandType = CommandType.Text;

                    //Definimos los parametros
                    cmd.Parameters.AddWithValue("@ClientName", master.ClientName);
                    cmd.Parameters.AddWithValue("@RegisteredDate", master.RegisteredDate);
                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                    cmd.Parameters.AddWithValue("@Discount", master.Discount);
                    cmd.Parameters.AddWithValue("@PaymentMethodId", master.PaymentMethodId);

                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));
                    detailsTable.Columns.Add("UnitPrice", typeof(decimal));

                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(item.ProductId, item.Quantity, item.UnitPrice);
                    }

                    SqlParameter detailParm = cmd.Parameters.AddWithValue("@SalesDetails", detailsTable);
                    detailParm.SqlDbType = SqlDbType.Structured;
                    detailParm.TypeName = "SalesDetalilsType";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            trasaction.Sale = new Sale
                            {
                                SaleId = (int)reader["InvoicesId"],
                                ClientName = reader["ClientName"].ToString(),
                                RegisteredDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["RegisteredDate"])),
                                SubTotal = (decimal)reader["SubTotal"],
                                Discount = (decimal)reader["Discount"],
                                Total = (decimal)reader["Total"],
                                UserId = (int)reader["UserId"],
                                PaymentMethodId = (int)reader["PaymentMethodId"]
                            };
                        }

                        await reader.NextResultAsync();
                        var saleDetailsList = new List<SaleDetails>();
                        while (await reader.ReadAsync())
                        {
                            saleDetailsList.Add(new SaleDetails
                            {
                                SalesDetailId = (int)reader["SalesDetailId"],
                                SaleId = (int)reader["InvoiceId"],
                                ProductId = (int)reader["ProductId"],
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"],
                                SubTotal = (decimal)reader["SubTotal"],
                                RegisteredDate = Convert.ToDateTime(reader["RegisteredDate"])
                            });

                        }

                        trasaction.SaleDetailsList = saleDetailsList;
                    }

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = trasaction,
                        OperationStatusCode = 0,
                        Message = "Venta registrada exitosamente."
                    };


                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

    }
}
