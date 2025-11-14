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
    public class RolesRepository : IRolesRepository
    {
        private readonly string _connectionString;
        public RolesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Roles>> AddAsync(Roles roles)
        {
            var response = new Roles();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddRol", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@RolName", roles.RolName);
                    //cmd.Parameters.AddWithValue("@IsActive", roles.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // leemos lo recien ingresado para mandar la respuesta con el nuevo registro... se lo mandamos al servicio para el mapeoS
                            response.Id = (int)reader["RolId"];
                            response.RolName = (string)reader["RolName"];
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }


                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Roles>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue

                    };
                }


            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }
        }

        public async Task<RepositoryResponse<IEnumerable<Roles>>> GetAllAsync()
        {
            var roles = new List<Roles>();
            var response = new RepositoryResponse<IEnumerable<Roles>>();
            try
            {
                // establecemos la conexion con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("UspGetAllRoles", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["IsActive"]

                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = roles;
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
                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Roles>> GetByIdAsync(int id)
        {
            var response = new Roles();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("Usp_GetRoleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.Id = (int)reader["RolId"];
                            response.RolName = reader["RolName"].ToString()!;
                            response.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<Roles>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }

        public async Task<RepositoryResponse<Roles>> UpdateAsync(int id, Roles roles)
        {
            try
            {
                var response = new RepositoryResponse<Roles>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.AddWithValue("@RolName", roles.RolName);
                    cmd.Parameters.AddWithValue("@IsActive", roles.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    Roles rolUpdate = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rolUpdate = new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };

                        }
                    }
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = rolUpdate;
                    response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Roles>
                    {
                        Data = rolUpdate,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };
            }
        }

        public async Task<RepositoryResponse<Roles>> GetByNameAsync(string name)
        {
            var rol = new Roles();
            var response = new RepositoryResponse<Roles>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetRolByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rol.Id = (int)reader["RolId"];
                            rol.RolName = (string)reader["RolName"];
                            rol.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = rol;
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

        public async Task<RepositoryResponse<Roles>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_DeactivateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Roles Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["IsActive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Roles>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
