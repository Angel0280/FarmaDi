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


public class ConcentrationRepository // : IConcentrationRepository// Revisar la interfaz 
{
    private readonly string _connectionString;

   public ConcentrationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    /*
    // Agregar una concentración
    public async Task<RepositoryResponse<Concentration>> AddAsync(Concentration concentration)
    {
        var response = new Concentration();
        try 
        {

            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand("USP_Concentration", connection);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@ConcentrationName", concentration.ConcentrationName);
                cmd.Parameters.AddWithValue("@ConcentrationDescription", concentration.ConcentrationDescription);
                cmd.Parameters.AddWithValue("@IsActive", concentration.IsActive);

                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                await cmd.ExecuteNonQueryAsync();
                var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                response.Data = returnedValue;
                response.OperationStatusCode = 0;


            }
        }
        catch(SqlException ex)
        {
            response.Data = -1;
            response.OperationStatusCode = ex.Number;

        }

        return response;
    }

    // Obtener todas las concentraciones
    public async Task<RepositoryResponse<IEnumerable<Concentration>>> GetAllAsync()
    {
        var list = new List<Concentration>();
        var response = new RepositoryResponse<IEnumerable<Concentration>>();

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand("USP_GetConcentrations", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Concentration
                        {
                            ConcentrationId = (int)reader["ConcentrationId"],
                            ConcentrationName = reader["ConcentrationName"].ToString()!,
                            ConcentrationDescription = reader["ConcentrationDescription"].ToString(),
                            IsActive = (bool)reader["IsActive"]
                        });
                    }
                }

                var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                response.Data = list;
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
            response.Data = null;
            response.OperationStatusCode = -1;
            response.Message = ex.Message;
        }

        return response;
    }

    // Obtener concentración por ID
    public async Task<RepositoryResponse<Concentration>> GetByIdAsync(int id)
    {
        var response = new RepositoryResponse<int>();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand("USP_GetConcentrationById", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ConcentrationId", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

               

                var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                return new RepositoryResponse<Concentration>
                {
                    Data = response,
                    OperationStatusCode = returnedValue
                };
            }
        }
        catch (SqlException ex)
        {
            return new RepositoryResponse<Concentration>
            {
                Data = null,
                OperationStatusCode = ex.Number,
                Message = ex.Message
            };
        }
    }

    // Actualizar concentración
    public async Task<RepositoryResponse<Concentration>> UpdateAsync(int id, Concentration concentration)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand("USP_UpdateConcentration", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ConcentrationId", id);
                cmd.Parameters.AddWithValue("@ConcentrationName", concentration.ConcentrationName);
                cmd.Parameters.AddWithValue("@ConcentrationDescription", concentration.ConcentrationDescription);
                cmd.Parameters.AddWithValue("@IsActive", concentration.IsActive);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                Concentration updated = null;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        updated = new Concentration
                        {
                            ConcentrationId = (int)reader["ConcentrationId"],
                            ConcentrationName = reader["ConcentrationName"].ToString()!,
                            ConcentrationDescription = reader["ConcentrationDescription"].ToString(),
                            IsActive = (bool)reader["IsActive"]
                        };
                    }
                }

                return new RepositoryResponse<Concentration>
                {
                    Data = updated,
                    OperationStatusCode = updated != null ? 0 : 1
                };
            }
        }
        catch (SqlException ex)
        {
            return new RepositoryResponse<Concentration>
            {
                Data = null,
                OperationStatusCode = ex.Number,
                Message = ex.Message
            };
        }
    }

    // Cambiar estado (Activar/Desactivar)
    public async Task<RepositoryResponse<Concentration>> SetStateAsync(int id, bool state)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand cmd = new SqlCommand("USP_UpdateConcentrationStatus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ConcentrationId", id);
                cmd.Parameters.AddWithValue("@IsActive", state);

                Concentration updated = null;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        updated = new Concentration
                        {
                            ConcentrationId = (int)reader["ConcentrationId"],
                            ConcentrationName = reader["ConcentrationName"].ToString(),
                            ConcentrationDescription = reader["ConcentrationDescription"].ToString(),
                            IsActive = (bool)reader["IsActive"]
                        };
                    }
                }

                return new RepositoryResponse<Concentration>
                {
                    Data = updated,
                    OperationStatusCode = updated != null ? 0 : 1
                };
            }
        }
        catch (Exception ex)
        {
            return new RepositoryResponse<Concentration>
            {
                Data = null,
                OperationStatusCode = -1,
                Message = ex.Message
            };
        }
    }

    public Task<RepositoryResponse<Concentration>> GetByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    */
}
