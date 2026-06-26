using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class PresentationRepository : IPresentationRepository
    {
        private readonly string _connectionString;

        public PresentationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Presentations>> AddAsync(Presentations presentation)
        {
            var presentationResult = new Presentations();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddPresentation", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PresentationDescription", presentation.Description);
                    cmd.Parameters.AddWithValue("@Quantity", presentation.Quantity);
                    cmd.Parameters.AddWithValue("@UnitMeasure", presentation.UnitMeasure);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            presentationResult.Id = (int)reader["PresentationId"];
                            presentationResult.Description = reader["PresentationDescription"].ToString()!;
                            presentationResult.Quantity = reader["quantity"].ToString()!;
                            presentationResult.UnitMeasure = reader["UnitMeasure"].ToString()!;
                            presentationResult.IsActive = true;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Presentations>
                    {
                        Data = presentationResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<Presentations>>> GetAllAsync()
        {
            var presentationList = new List<Presentations>(); // CORREGIDO: Antes se llamaba 'category'
            var response = new RepositoryResponse<IEnumerable<Presentations>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllPresentations", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            presentationList.Add(new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString()!,
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = presentationList;
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
                return new RepositoryResponse<IEnumerable<Presentations>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Presentations>> GetByIdAsync(int id)
        {
            var presentationResult = new Presentations();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetPresentationById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PresentationId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            presentationResult.Id = (int)reader["PresentationId"];
                            presentationResult.Description = reader["PresentationDescription"].ToString()!;
                            presentationResult.Quantity = reader["quantity"].ToString()!;
                            presentationResult.UnitMeasure = reader["UnitMeasure"].ToString()!;
                            presentationResult.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Presentations>
                    {
                        Data = presentationResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Presentations>> UpdateAsync(int id, Presentations presentation)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdatePresentation", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PresentationId", id);
                    cmd.Parameters.AddWithValue("@PresentationDescription", presentation.Description);
                    cmd.Parameters.AddWithValue("@Quantity", presentation.Quantity);
                    cmd.Parameters.AddWithValue("@UnitMeasure", presentation.UnitMeasure);
                    cmd.Parameters.AddWithValue("@IsActive", presentation.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    Presentations updatedPresentation = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedPresentation = new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString()!,
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Presentations>
                    {
                        Data = updatedPresentation,
                        OperationStatusCode = 0
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Presentations>> GetByNameAsync(string name)
        {
            var presentationResult = new Presentations();
            var response = new RepositoryResponse<Presentations>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // CORREGIDO: Apunta al SP de presentaciones, no de categorías
                    SqlCommand cmd = new SqlCommand("USP_GetPresentationByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PresentationDescription", name); // CORREGIDO: Parámetro correcto
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            presentationResult.Id = (int)reader["PresentationId"];
                            presentationResult.Description = reader["PresentationDescription"].ToString()!;
                            presentationResult.Quantity = reader["quantity"].ToString()!;
                            presentationResult.UnitMeasure = reader["UnitMeasure"].ToString()!;
                            presentationResult.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = presentationResult;
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

        public async Task<RepositoryResponse<Presentations>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_DeactivatePresentation", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PresentationId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Presentations updatedPresentation = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedPresentation = new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString()!,
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Presentations>
                    {
                        Data = updatedPresentation,
                        OperationStatusCode = updatedPresentation != null ? 0 : 1
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}