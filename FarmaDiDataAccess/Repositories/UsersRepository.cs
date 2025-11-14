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
using System.Transactions;

namespace FarmaDiDataAccess.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly string _connectionString;
        public UsersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Users>> AddAsync(Users users)
        {
            var response = new Users();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddRol", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", users.UserName);
                    cmd.Parameters.AddWithValue("@UserLastName", users.UserLastName);
                    cmd.Parameters.AddWithValue("@PasswordHash", users.PasswordHash);
                    cmd.Parameters.AddWithValue("@Mail", users.Mail);
                    cmd.Parameters.AddWithValue("UserPhone", users.UserPhone);
                    cmd.Parameters.AddWithValue("IsActive", users.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.UserName = reader["UserName"].ToString()!;
                            response.UserLastName = reader["UserLastName"].ToString()!;
                            response.PasswordHash = reader["UserPassword"].ToString()!;
                            response.Mail = reader["Mail"].ToString()!;
                            response.UserPhone = reader["UserPhone"].ToString()!;
                            response.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Users>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue

                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message

                };

            }
        }

        public async Task<RepositoryResponse<IEnumerable<Users>>> GetAllAsync()
        {
            var users = new List<Users>();
            var response = new RepositoryResponse<IEnumerable<Users>>();
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
                            users.Add(new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = users;
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
                return new RepositoryResponse<IEnumerable<Users>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Users>> GetByIdAsync(int id)
        {
            Users response = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("UspGetRoleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.UserId = (int)reader["RolId"];
                            response.UserName = reader["RolName"].ToString()!;
                            response.UserLastName = reader["UserLastName"].ToString()!;
                            response.Mail = reader["Mail"].ToString()!;
                            response.UserPhone = reader["UserPhone"].ToString()!;
                            response.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<Users>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

        }

        public async Task<RepositoryResponse<Users>> UpdateAsync(int id, Users users)
        {
            try
            {
                var response = new RepositoryResponse<Users>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.AddWithValue("@RolName", users.UserName);
                    cmd.Parameters.AddWithValue("@IsActive", users.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                    Users userUpdate = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userUpdate = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = userUpdate;
                    response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Users>
                    {
                        Data = userUpdate,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };
            }
        }

        public async Task<RepositoryResponse<Users>> GetByNameAsync(string name)
        {
            Users user = null;
            var response = new RepositoryResponse<Users>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetRolByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.UserId = (int)reader["UserId"];
                            user.UserName = reader["UserName"].ToString()!;
                            user.UserLastName = reader["UserLastName"].ToString()!;
                            user.Mail = reader["Mail"].ToString()!;
                            user.UserPhone = reader["UserPhone"].ToString()!;
                            user.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = user;
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

        public async Task<RepositoryResponse<Users>> GetByUserNameAsync(string name)
        {
            var user = new Users();

            var response = new RepositoryResponse<Users>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.UserId = (int)reader["UserId"];
                            user.UserName = reader["UserName"].ToString()!;
                            user.UserLastName = reader["UserLastName"].ToString()!;
                            user.Mail = reader["Mail"].ToString()!;
                            user.PasswordHash = reader["UserPassword"].ToString()!;
                            user.UserPhone = reader["UserPhone"].ToString()!;
                            user.IsActive = (bool)reader["IsActive"];

                        }
                        else
                        {
                            user = null;
                        }
                    }
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = user;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
                catch (SqlException ex)
                {
                    response.Data = null;
                    response.OperationStatusCode = ex.Number;
                    return response;
                }
            }
        }

        public async Task<RepositoryResponse<Users>> GetByEmailAsync(string email)
        {
            Users user = null;
            var response = new RepositoryResponse<Users>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserByEmail", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mail", email);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.UserId = (int)reader["UserId"];
                            user.UserName = reader["UserName"].ToString()!;
                            user.UserLastName = reader["UserLastName"].ToString()!;
                            user.PasswordHash = reader["PasswordHash"].ToString()!;
                            user.Mail = reader["Mail"].ToString()!;
                            user.UserPhone = reader["UserPhone"].ToString()!;
                            user.IsActive = (bool)reader["IsActive"];
                        }
                    }
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = user;
                    response.OperationStatusCode = returnedValue;

                    return response;

                }
                catch (SqlException ex)
                {
                    response.Data = null;
                    response.OperationStatusCode = ex.Number;
                    return response;
                }



            }
        }

        public async Task<RepositoryResponse<Users>> SetStateAsync(int id, bool state)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateBrandStatus", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Users Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Users>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<RolesUers>> RegisterUserWithRolesAsync(Users user, IEnumerable<Roles> roleIds)
        {

            var response = new RolesUers();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_RegisterUserWithRoles", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@UserLastName", user.UserLastName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Mail", user.Mail);
                    cmd.Parameters.AddWithValue("UserPhone", user.UserPhone);
                    cmd.Parameters.AddWithValue("IsActive", user.IsActive);
                    // Crear un DataTable para los IDs de roles
                    var roleIdsTable = new DataTable();
                    roleIdsTable.Columns.Add("RollId", typeof(int));
                    // Llenar el DataTable con los IDs de roles
                    foreach (var items in roleIds)
                    {
                        roleIdsTable.Rows.Add(items.Id);
                    }

                    // Agregar el parámetro de tabla para los IDs de roles
                    SqlParameter rolParam = cmd.Parameters.AddWithValue("@Roles", roleIdsTable);
                    rolParam.SqlDbType = SqlDbType.Structured;
                    rolParam.TypeName = "TipoListaRoles";
                    //cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.Users = new Users {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]

                            };
                        }

                        await reader.NextResultAsync();
                        //Creamos un objeto de tipo Lista para almacenar los roles asociados al usuario
                        var rolesList = new List<Roles>();
                        while (await reader.ReadAsync())
                        {
                            rolesList.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                
                            });
                        }

                        response.Roles = rolesList;

                    }

                    return new RepositoryResponse<RolesUers>
                    {
                        Data = response,
                        OperationStatusCode = 0,
                        Message = "Operacion exitosa"
                    };
                } 
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<RolesUers>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }

            catch (Exception ex)
            {
                return new RepositoryResponse<RolesUers>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
