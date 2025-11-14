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
    public class SuppliersRepository:ISuppliersRepository
    {
        private readonly string _connectionString;
        public SuppliersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }


        // aqui agregamos una marca a la base de datos
        public async Task<RepositoryResponse<Suppliers>> AddAsync(Suppliers supplier)
        {
            var response = new Suppliers();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddSupplier", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                    cmd.Parameters.AddWithValue("@Rnc", supplier.RNC);
                    cmd.Parameters.AddWithValue("@Mail", supplier.Mail);
                    cmd.Parameters.AddWithValue("@Phone", supplier.SupplierPhone);
                    cmd.Parameters.AddWithValue("@Address", supplier.SupplierAddress);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.SupplierId = (int)reader["supplierId"];
                            response.SupplierName = reader["SupplierName"].ToString()!;
                            response.RNC = reader["Rnc"].ToString();
                            response.Mail = reader["Mail"].ToString();
                            response.SupplierPhone = reader["SupplierPhone"].ToString();
                            response.SupplierAddress = reader["SupplierAddress"].ToString();
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }


                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Suppliers>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue

                    };
                }


            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Suppliers>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }
        }
 
        public async Task<RepositoryResponse<IEnumerable<Suppliers>>> GetAllAsync()
        {
            var Suppliers = new List<Suppliers>();
            var response = new RepositoryResponse<IEnumerable<Suppliers>>();
            try
            {
                // establecemos la conexion con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllSuppliers", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Suppliers.Add(new Suppliers
                            {
                                SupplierId = (int)reader["SupplierId"],
                                SupplierName = reader["SupplierName"].ToString()!,
                                RNC = reader["Rnc"].ToString(),
                                Mail = reader["Mail"].ToString(),
                                SupplierPhone = reader["SupplierPhone"].ToString(),
                                SupplierAddress = reader["SupplierAddress"].ToString(),
                                IsActive = (bool)reader["IsActive"]
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = Suppliers;
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
                return new RepositoryResponse<IEnumerable<Suppliers>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }


        public async Task<RepositoryResponse<Suppliers>> GetByIdAsync(int id)
        {
            var response = new Suppliers();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetSupplierById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                         
                            response.SupplierId = (int)reader["SupplierId"];
                            response.SupplierName = reader["SupplierName"].ToString()!;
                            response.RNC = reader["Rnc"].ToString();
                            response.Mail = reader["Mail"].ToString();
                            response.SupplierPhone = reader["SupplierPhone"].ToString();
                            response.SupplierAddress = reader["SupplierAddress"].ToString();
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<Suppliers>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Suppliers>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }


        public async Task<RepositoryResponse<Suppliers>> UpdateAsync(int id, Suppliers Suppliers)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateSupplier", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierId", id);
                    cmd.Parameters.AddWithValue("@SupplierName", Suppliers.SupplierName);
                    cmd.Parameters.AddWithValue("@Rnc", Suppliers.RNC);
                    cmd.Parameters.AddWithValue("@Mail", Suppliers.Mail);
                    cmd.Parameters.AddWithValue("@Phone", Suppliers.SupplierPhone);
                    cmd.Parameters.AddWithValue("@Address", Suppliers.SupplierAddress);
                    cmd.Parameters.AddWithValue("@Isactive", Suppliers.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    Suppliers supplierUpdate = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            supplierUpdate = new Suppliers
                            {
                                SupplierId = (int)reader["SupplierId"],
                                SupplierName = reader["SupplierName"].ToString()!,
                                RNC = reader["Rnc"].ToString(),
                                Mail = reader["Mail"].ToString(),
                                SupplierPhone = reader["SupplierPhone"].ToString(),
                                SupplierAddress = reader["SupplierAddress"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }
                    // var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    // response.Data = supplierUpdate;
                    // response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Suppliers>
                    {
                        Data = supplierUpdate,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Suppliers>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };
            }
        }



        public async Task<RepositoryResponse<Suppliers>> GetByNameAsync(string name)
        {
            var supplier = new Suppliers();
            var response = new RepositoryResponse<Suppliers>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetSupplierByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            supplier.SupplierId = (int)reader["SupplierId"];
                            supplier.SupplierName = reader["SupplierName"].ToString()!;
                            supplier.RNC = reader["Rnc"].ToString();
                            supplier.Mail = reader["Mail"].ToString();
                            supplier.SupplierPhone = reader["SupplierPhone"].ToString();
                            supplier.SupplierAddress = reader["SupplierAddress"].ToString();
                            supplier.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = supplier;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                return response;
            }
        }


        public async Task<RepositoryResponse<Suppliers>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateSupplierStatus", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Suppliers Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Suppliers
                            {
                                SupplierId = (int)reader["SupplierId"],
                                SupplierName = reader["SupplierName"].ToString(),
                                RNC = reader["Rnc"].ToString(),
                                Mail = reader["Mail"].ToString(),
                                SupplierPhone = reader["SupplierPhone"].ToString(),
                                SupplierAddress = reader["SupplierAddress"].ToString(),
                                IsActive = (bool)reader["IsActive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Suppliers>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Suppliers>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }




    }
}
