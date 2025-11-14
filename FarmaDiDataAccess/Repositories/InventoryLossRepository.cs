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
    public class InventoryLossRepository: IInventoryLossRepository
    {
        private readonly string _connectionString;
        public InventoryLossRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<InventoryLoss>>> GetAllAsync()
        {
            var inventoryLoss = new List<InventoryLoss>();
            var response = new RepositoryResponse<IEnumerable<InventoryLoss>>();
            try
            {
                // establecemos la conexion con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventoryLoss", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            inventoryLoss.Add(new InventoryLoss
                            {
                                LowId = (int)reader["LowId"],
                                oBatch = new ProductBatches{ Id = (int) reader["BatchId"], BatchNumer = reader["BatchNumber"].ToString()},
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products { ProductId = (int) reader["ProductId"], GenericName = reader["ProductGenericName"].ToString(), TradeName= reader ["ProductTradeName"].ToString() },
                                oUser= new Users { UserId = (int) reader["UserId"], UserName = reader["UserName"].ToString() },
                                Reason = reader["Reason"].ToString(),
                                //IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = inventoryLoss;
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
                return new RepositoryResponse<IEnumerable<InventoryLoss>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }


        public async Task<RepositoryResponse<InventoryLoss>> GetByIdAsync(int id)
        {
            var response = new InventoryLoss();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryLossById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LowId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.LowId = (int)reader["LowId"];
                            response.oBatch = new ProductBatches { Id = (int)reader["BatchId"], BatchNumer = reader["BatchNumber"].ToString() };
                            response.Quantity = (int)reader["Quantity"];
                            response.oProduct = new Products { ProductId = (int)reader["ProductId"], GenericName = reader["ProductGenericName"].ToString(), TradeName = reader["ProductTradeName"].ToString() };
                            response.oUser = new Users { UserId = (int)reader["UserId"], UserName = reader["UserName"].ToString() };
                            response.Reason = reader["Reason"].ToString();

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<InventoryLoss>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryLoss>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }


        public async Task<RepositoryResponse<InventoryLoss>> AddAsync(InventoryLoss inventoryLoss)
        {
            var response = new InventoryLoss();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddInventoryLoss", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BatchId", inventoryLoss.BatchId);
                    cmd.Parameters.AddWithValue("@Quantity", inventoryLoss.Quantity);
                    cmd.Parameters.AddWithValue("@ProductId", inventoryLoss.ProductId);
                    cmd.Parameters.AddWithValue("@UserId", inventoryLoss.UserId);
                    cmd.Parameters.AddWithValue("@Reason", inventoryLoss.Reason);
                    //cmd.Parameters.AddWithValue("@IsActive", brand.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.LowId = (int)reader["BatchId"];
                            response.oBatch = new ProductBatches { Id = (int)reader["BatchId"]  };
                            response.Quantity = (int) reader["Quantity"];
                            response.oProduct =  new Products{ ProductId = (int)reader["ProductId"] };
                            response.oUser = new Users { UserId = (int)reader["UserId"] };
                            response.Reason = reader["Reason"].ToString();

                        }
                    }

                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<InventoryLoss>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue

                    };
                }


            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryLoss >
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }
        }
      



    }
}
