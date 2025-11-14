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
    public class BrandsRepository :IBrandsRepository
    {
        private readonly string _connectionString;
        public BrandsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }


        // aqui agregamos una marca a la base de datos
        public async Task<RepositoryResponse<Brands>> AddAsync(Brands brand)
        {
            var response = new Brands();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("Usp_AddBrand", connection);
                      cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BrandName", brand.BrandName);
                    cmd.Parameters.AddWithValue("@BrandDescription", brand.Description);
                    //cmd.Parameters.AddWithValue("@IsActive", brand.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.BrandId = (int)reader["BrandId"];
                            response.BrandName = reader["BrandName"].ToString()!;
                            response.Description = reader["BrandDescription"].ToString();
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }


                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Brands>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue

                    };
                }


            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Brands>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }
        }      


        // aqui mandamos a llamar todos los registros existentes en "marcas", si no hay registros
        // mandamos un código personalizado 50009 --  
        public async Task<RepositoryResponse<IEnumerable<Brands>>> GetAllAsync()
        {
            var brands = new List<Brands>();
            var response = new RepositoryResponse<IEnumerable<Brands>>();
            try
            {
                // establecemos la conexion con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetBrands", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            brands.Add(new Brands
                            {
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString()!,
                                Description = reader["BrandDescription"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = brands;
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
                return new RepositoryResponse<IEnumerable<Brands>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }


        public async Task<RepositoryResponse<Brands>> GetByIdAsync(int id)
        {
            var response = new Brands();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("Usp_GetBrandById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.BrandId = (int)reader["BrandId"];
                            response.BrandName = reader["BrandName"].ToString();
                            response.Description = reader["BrandDescription"].ToString();
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
              

                    return new RepositoryResponse<Brands>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Brands>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }


        public async Task<RepositoryResponse<Brands>> UpdateAsync(int id, Brands brands)
        {            
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateBrand", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandId",id);
                    cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                    cmd.Parameters.AddWithValue("@BrandDescription", brands.Description);
                    cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    Brands  brandUpdate = null;  
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            brandUpdate = new Brands
                            {
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString()!,
                                Description = reader["BrandDescription"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }
                    // var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    // response.Data = brandUpdate;
                    // response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Brands>
                    {
                        Data = brandUpdate,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Brands>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };        
            }
        }



        public async Task<RepositoryResponse<Brands>> GetByNameAsync(string name)
        {
            var brand = new Brands();
            var response = new RepositoryResponse<Brands>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetBrandByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            brand.BrandId = (int)reader["BrandId"];
                            brand.BrandName = reader["BrandName"].ToString()!;
                            brand.Description = reader["BrandDescription"].ToString();
                            brand.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = brand;
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


        public async Task<RepositoryResponse<Brands>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateBrandStatus", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Brands Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Brands
                            {
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString(),
                                Description = reader["BrandDescription"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Brands>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Brands>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }



    }
}
