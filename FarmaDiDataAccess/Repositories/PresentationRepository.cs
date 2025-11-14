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
    public class PresentationRepository: IPresentationRepository
    {
        private readonly string _connectionString;
        public PresentationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // aqui agregamos una presentación a la base de datos
        public async Task<RepositoryResponse<Presentations>> AddAsync(Presentations presentation)
        {
            var response = new Presentations();
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
                    //cmd.Parameters.AddWithValue("@IsActive", presentation.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.Id = (int)reader["PresentationId"];
                            response.Description = reader["PresentationDescription"].ToString();
                            response.Quantity = reader["quantity"].ToString()!;
                            response.UnitMeasure = reader["UnitMeasure"].ToString()!;
                          //  response.IsActive = (bool)reader["Isactive"];


                        }
                    }


                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Presentations>
                    {
                        Data = response,
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
        }

        public async Task<RepositoryResponse<IEnumerable<Presentations>>> GetAllAsync()
        {
            var category = new List<Presentations>();
            var response = new RepositoryResponse<IEnumerable<Presentations>>();
            try
            {
                // establecemos la conexion con la base de datos
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
                            category.Add(new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString(),
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]

                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = category;
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
            var response = new Presentations();
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
                            response.Id = (int)reader["PresentationId"];
                            response.Description = reader["PresentationDescription"].ToString();
                            response.Quantity = reader["quantity"].ToString()!;
                            response.UnitMeasure = reader["UnitMeasure"].ToString()!;
                            response.IsActive = (bool)reader["Isactive"];

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<Presentations>
                    {
                        Data = response,
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


                    Presentations Update = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Update = new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString(),
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }
                    // var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    // response.Data = brandUpdate;
                    // response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Presentations>
                    {
                        Data = Update,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Presentations>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };
            }
        }

        public async Task<RepositoryResponse<Presentations>> GetByNameAsync(string name)
        {
            var presentation = new Presentations();
            var response = new RepositoryResponse<Presentations>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetCategoryByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CategoryName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            presentation.Id = (int)reader["PresentationId"];
                            presentation.Description = reader["PresentationDescription"].ToString();
                            presentation.Quantity = reader["quantity"].ToString()!;
                            presentation.UnitMeasure = reader["UnitMeasure"].ToString()!;
                            presentation.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = presentation;
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

                    Presentations Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Presentations
                            {
                                Id = (int)reader["PresentationId"],
                                Description = reader["PresentationDescription"].ToString(),
                                Quantity = reader["quantity"].ToString()!,
                                UnitMeasure = reader["UnitMeasure"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Presentations>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
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
