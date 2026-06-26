using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly string _connectionString;

        public SalesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetails> details)
        {
            var saleTransaction = new SaleTransaction();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_InsertSale", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Parámetros del Maestro (Coincidencia exacta con los 5 parámetros del SP)
                    cmd.Parameters.AddWithValue("@ClientName", master.ClientName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                    cmd.Parameters.AddWithValue("@Discount", master.Discount);
                    cmd.Parameters.AddWithValue("@PaymentMethodId", master.PaymentMethodId);

                    // 2. Estructuración del DataTable del Detalle (Sincronizado a 2 columnas)
                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("Quantity", typeof(int));
                    detailsTable.Columns.Add("ProductId", typeof(int));
                   

                    foreach (var item in details)
                    {
                        // Pasamos únicamente los 2 valores que espera tu UDTT
                        detailsTable.Rows.Add( item.Quantity, item.ProductId);
                    }

                    // El 5to parámetro del SP: El tipo estructurado
                    SqlParameter detailParm = cmd.Parameters.AddWithValue("@SalesDetails", detailsTable);
                    detailParm.SqlDbType = SqlDbType.Structured;
                    detailParm.TypeName = "SalesDetalilsType";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // LECTURA DEL MAESTRO DE VENTA (Result Set 1: Cabecera de Invoices)
                        if (await reader.ReadAsync())
                        {
                            saleTransaction.Sale = new Sale
                            {
                                SaleId = (int)reader["InvoiceId"], // Mapeado al InvoiceId devuelto por tu SELECT * FROM Invoices
                                UserId = (int)reader["UserId"],
                                ClientName = reader["ClientName"] is DBNull ? null : reader["ClientName"].ToString(),
    //                            RegisteredDate = Convert.ToDateTime(reader["RegisteredDate"]), // Descomentado de forma segura
                                Discount = (decimal)reader["Discount"],
                                SubTotal = (decimal)reader["SubTotal"],
                                Total = (decimal)reader["Total"],
                                PaymentMethodId = master.PaymentMethodId,
                            };
                        }

                        // Pasar al segundo Result Set (Detalles de la Factura)
                        await reader.NextResultAsync();

                        var saleDetailsList = new List<SaleDetails>();
                        while (await reader.ReadAsync())
                        {
                            saleDetailsList.Add(new SaleDetails
                            {
                                SalesDetailId = (int)reader["InvoicesDetailId"], // Coincide con tu SELECT del SP
                                SaleId = (int)reader["InvoiceId"],
                                ProductId = (int)reader["ProductId"],
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"], // Calculado implícitamente en el SP
                                SubTotal = (decimal)reader["TotalPrice"], // TotalPrice del detalle mapeado al SubTotal del objeto
                                //RegisteredDate = saleTransaction.Sale.RegisteredDate // Sincronizado con la fecha de la cabecera
                            });
                        }

                        saleTransaction.SaleDetailsList = saleDetailsList;
                    }

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = saleTransaction,
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
                    OperationStatusCode = ex.Number, // Si el THROW 51001 de stock insuficiente se dispara, lo atrapas aquí
                    Message = $"Error de Base de Datos ({ex.Number}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = $"Error general en infraestructura de ventas: {ex.Message}"
                };
            }
        }



        public async Task<RepositoryResponse<SaleTransaction>> GetInvoiceByIdAsync(int invoiceId)
        {
            if (invoiceId <= 0)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = 400,
                    Message = "El ID de factura debe ser mayor a cero."
                };
            }

            var transaction = new SaleTransaction();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var cmd = new SqlCommand("USP_GetInvoiceById", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;

                await using var reader = await cmd.ExecuteReaderAsync();

                // 1. Leer Cabecera (1 solo registro)
                if (!await reader.ReadAsync())
                {
                    // Si el SP no encontró nada o lanzó RAISERROR, puede caer aquí
                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = null,
                        OperationStatusCode = 404,
                        Message = "Factura no encontrada."
                    };
                }

                transaction.InvoiceMaster = MapInvoiceHeader(reader);

                // 2. Leer Detalles (múltiples registros)
                if (await reader.NextResultAsync())
                {
                    var detailsList = new List<InvoiceDetails>();
                    while (await reader.ReadAsync())
                    {
                        detailsList.Add(MapInvoiceDetail(reader));
                    }
                    transaction.InvoiceDetails = detailsList;
                }
                else
                {
                    transaction.InvoiceDetails = new List<InvoiceDetails>();
                }

                return new RepositoryResponse<SaleTransaction>
                {
                    Data = transaction,
                    OperationStatusCode = 0,
                    Message = "Factura recuperada exitosamente."
                };
            }
            catch (SqlException ex) when (ex.Number == 50000) // RAISERROR del SP
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = 400,
                    Message = ex.Message
                };
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = $"Error de Base de Datos ({ex.Number}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = $"Error general en infraestructura de ventas: {ex.Message}"
                };
            }
        }

        // Helpers privados para evitar repetir código
        private Invoice MapInvoiceHeader(SqlDataReader reader)
        {
            return new Invoice
            {
                InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                ClientName = reader.IsDBNull(reader.GetOrdinal("ClientName")) ? null : reader.GetString(reader.GetOrdinal("ClientName")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
               // UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName")), // Nuevo campo del SP
                RegisteredDate = reader.GetDateTime(reader.GetOrdinal("RegisteredDate")),
                Discount = reader.GetDecimal(reader.GetOrdinal("Discount")),
                SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
               // IsPrinted = reader.GetBoolean(reader.GetOrdinal("IsPrinted"))
            };
        }

        private InvoiceDetails MapInvoiceDetail(SqlDataReader reader)
        {
            return new InvoiceDetails
            {
                InvoicesDetailId = reader.GetInt32(reader.GetOrdinal("InvoicesDetailId")),
                InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                ProductTradeName = reader.IsDBNull(reader.GetOrdinal("ProductTradeName")) ? null : reader.GetString(reader.GetOrdinal("ProductTradeName")),
                ProductGenericName = reader.IsDBNull(reader.GetOrdinal("ProductGenericName")) ? null : reader.GetString(reader.GetOrdinal("ProductGenericName")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"))
            };
        }


        public async Task<RepositoryResponse<PagedSaleResult>> GetSalesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var result = new PagedSaleResult
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Invoices = new List<Invoice>()
            };

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Asegúrate de cambiar el nombre al SP que creamos: sp_sales_get_paged
                await using var cmd = new SqlCommand("sp_sales_get_paged", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros limpios del SP
                cmd.Parameters.Add("@page_number", SqlDbType.Int).Value = pageNumber;
                cmd.Parameters.Add("@page_size", SqlDbType.Int).Value = pageSize;

                await using var reader = await cmd.ExecuteReaderAsync();

                var invoices = new List<Invoice>();
                int totalRecords = 0;

                while (await reader.ReadAsync())
                {
                    // Capturamos el TotalRows solo una vez desde la primera fila
                    if (totalRecords == 0)
                    {
                        totalRecords = reader.GetInt32(reader.GetOrdinal("TotalRows"));
                    }

                    invoices.Add(new Invoice
                    {
                        // Mapeado usando el alias 'SaleId' que pusimos en el SP
                        InvoiceId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                        ClientName = reader.IsDBNull(reader.GetOrdinal("ClientName")) ? null : reader.GetString(reader.GetOrdinal("ClientName")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        RegisteredDate = reader.GetDateTime(reader.GetOrdinal("RegisteredDate")),
                        Discount = reader.GetDecimal(reader.GetOrdinal("Discount")),
                        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                        Total = reader.GetDecimal(reader.GetOrdinal("Total"))
                    });
                }

                // Asignamos la data y calculamos las páginas totales para el frontend
                result.Invoices = invoices;
                result.TotalRecords = totalRecords;
                result.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                return new RepositoryResponse<PagedSaleResult>
                {
                    Data = result,
                    OperationStatusCode = 0,
                    Message = invoices.Any()
                        ? $"{invoices.Count} ventas recuperadas con éxito."
                        : "No se encontraron registros de ventas."
                };
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PagedSaleResult>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = $"Error de Base de Datos ({ex.Number}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PagedSaleResult>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = $"Error general en el repositorio de ventas: {ex.Message}"
                };
            }
        }


    }
}