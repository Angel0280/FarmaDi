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
    public class ProductBatchesRepository: IProductBatchesRepository
    {
        private readonly string _connectionString;
        public ProductBatchesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<ProductBatches>>> GetAllAsync()
        {
            var productBatches = new List<ProductBatches>();
            var response = new RepositoryResponse<IEnumerable<ProductBatches>>();
            try
            {
                // establecemos la conexion con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllProductBatches", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productBatches.Add(new ProductBatches
                            {
                                Id = (int)reader["BatchId"],
                                BatchNumer = reader["BatchNumber"].ToString()!,
                                ManufacturingDate = (DateTime)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"],
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products { ProductId = (int)reader["ProductId"], GenericName = reader["ProductGenericName"].ToString(), TradeName= reader["ProductTradeName"].ToString() },
                                IsActive = (bool)reader["Isactive"],
                                
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = productBatches;
                    response.OperationStatusCode = returnedValue;

                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
            }

            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<ProductBatches>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }


        public async Task<RepositoryResponse<ProductBatches>> GetByIdAsync(int id)
        {
            var response = new ProductBatches();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductBatchesById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BatchId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.Id = (int)reader["BatchId"];
                            response.BatchNumer = reader["BatchNumber"].ToString();
                            response.ManufacturingDate =(DateTime)reader["ManufacturingDate"];
                            response.ExpirationDate =(DateTime) reader["ExpirationDate"];
                            response.Quantity = (int)reader["Quantity"];
                            response.oProduct = new Products { ProductId = (int)reader["ProductId"], GenericName = reader["ProductGenericName"].ToString(), TradeName = reader["ProductTradeName"].ToString() };
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<ProductBatches>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<ProductBatches>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }

    }
}
