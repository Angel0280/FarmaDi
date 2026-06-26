 using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class BrandsRepository : IBrandsRepository
    {
        private readonly string _connectionString;

        public BrandsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Brands>> AddAsync(Brands brand)
        {
            var brandResult = new Brands();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("Usp_AddBrand", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BrandName", brand.BrandName);
                    cmd.Parameters.AddWithValue("@BrandDescription", brand.Description);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            brandResult.BrandId = (int)reader["BrandId"];
                            brandResult.BrandName = reader["BrandName"].ToString()!;
                            brandResult.Description = reader["BrandDescription"].ToString();
                            brandResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Brands>
                    {
                        Data = brandResult,
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

        public async Task<RepositoryResponse<IEnumerable<Brands>>> GetAllAsync()
        {
            var brandList = new List<Brands>();
            var response = new RepositoryResponse<IEnumerable<Brands>>();
            try
            {
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
                            brandList.Add(new Brands
                            {
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString()!,
                                Description = reader["BrandDescription"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = brandList;
                    response.OperationStatusCode = returnedValue;
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

        public async Task<RepositoryResponse<Brands>> GetByIdAsync(int id)
        {
            var brandResult = new Brands();
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
                            brandResult.BrandId = (int)reader["BrandId"];
                            brandResult.BrandName = reader["BrandName"].ToString();
                            brandResult.Description = reader["BrandDescription"].ToString();
                            brandResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Brands>
                    {
                        Data = brandResult,
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

        public async Task<RepositoryResponse<Brands>> UpdateAsync(int id, Brands brands)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateBrand", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandId", id);
                    cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                    cmd.Parameters.AddWithValue("@BrandDescription", brands.Description);
                    cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);

                    Brands brandUpdate = null;
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

                    return new RepositoryResponse<Brands>
                    {
                        Data = brandUpdate,
                        OperationStatusCode = 0
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

        public async Task<RepositoryResponse<Brands>> GetByNameAsync(string name)
        {
            var brandResult = new Brands();
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
                            brandResult.BrandId = (int)reader["BrandId"];
                            brandResult.BrandName = reader["BrandName"].ToString()!;
                            brandResult.Description = reader["BrandDescription"].ToString();
                            brandResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = brandResult;
                    response.OperationStatusCode = returnedValue;
                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
                return response;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
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

                    Brands updatedBrand = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedBrand = new Brands
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
                        Data = updatedBrand,
                        OperationStatusCode = updatedBrand != null ? 0 : 1
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

        public async Task<RepositoryResponse<(IEnumerable<Brands> Items, int TotalCount)>> GetBrandsPagedAsync(int page, int limit)
        {
            var brandList = new List<Brands>();
            var response = new RepositoryResponse<(IEnumerable<Brands> Items, int TotalCount)>();
            int totalRecords = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetBrandsPaged", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PageNumber", page);
                    cmd.Parameters.AddWithValue("@PageSize", limit);

                    SqlParameter totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int);
                    totalRecordsParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(totalRecordsParam);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            brandList.Add(new Brands
                            {
                                BrandId = (int)reader["BrandId"],
                                BrandName = reader["BrandName"].ToString()!,
                                Description = reader["BrandDescription"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalRecords = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    response.Data = (brandList, totalRecords);
                    response.OperationStatusCode = 0;
                }
            }
            catch (SqlException ex)
            {
                response.Data = (new List<Brands>(), 0);
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = (new List<Brands>(), 0);
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }
    }
} 