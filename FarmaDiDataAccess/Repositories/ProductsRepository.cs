using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly string _connectionString;

        public ProductsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Products>> AddAsync(Products product)
        {
            var productResult = new Products(); // CORREGIDO: Renombrado semántico
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProductTradeName", product.TradeName);
                    cmd.Parameters.AddWithValue("@ProductGenericName", product.GenericName);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    cmd.Parameters.AddWithValue("@PresentationId", product.PresentationId);
                    cmd.Parameters.AddWithValue("@ConcentrationId", product.ConcentrationId);
                    cmd.Parameters.AddWithValue("@ConcentrationValue", product.ConcentrationValue); // NUEVO
                    cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId);
                    cmd.Parameters.AddWithValue("@BrandId", product.BrandId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productResult.ProductId = (int)reader["ProductId"];
                            productResult.TradeName = reader["ProductTradeName"].ToString()!;
                            productResult.GenericName = reader["ProductGenericName"].ToString()!;
                            productResult.CategoryId = (int)reader["CategoryId"];
                            productResult.PresentationId = (int)reader["PresentationId"];
                            productResult.ConcentrationId = (int)reader["ConcentrationId"];
                            productResult.ConcentrationValue = reader["ConcentrationValue"].ToString(); // NUEVO
                            productResult.SupplierId = (int)reader["SupplierId"];
                            productResult.BrandId = (int)reader["BrandId"];
                            productResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Products>
                    {
                        Data = productResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Blindaje contra fallos de casteo en runtime
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<Products>>> GetAllAsync()
        {
            var productList = new List<Products>(); // CORREGIDO: Nomenclatura camelCase
            var response = new RepositoryResponse<IEnumerable<Products>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllProducts", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productList.Add(new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString()!,
                                oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString()! },
                                oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString()! },
                                oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], Volume = reader["Porcentage"].ToString()! },
                                ConcentrationValue = reader["ConcentrationValue"].ToString(), // NUEVO
                                oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString()! },
                                obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString()! },
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = productList;
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
                return new RepositoryResponse<IEnumerable<Products>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Products>> GetByIdAsync(int id)
        {
            var productResult = new Products();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productResult.ProductId = (int)reader["ProductId"];
                            productResult.GenericName = reader["ProductGenericName"].ToString()!;
                            productResult.TradeName = reader["ProductTradeName"].ToString()!;
                            productResult.oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString()! };
                            productResult.oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString()! };
                            productResult.oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], Volume = reader["Porcentage"].ToString()! };
                            productResult.ConcentrationValue = reader["ConcentrationValue"].ToString(); // NUEVO
                            productResult.oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString()! };
                            productResult.obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString()! };
                            productResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Products>
                    {
                        Data = productResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Bloque catch global de resguardo
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Products>> UpdateAsync(int id, Products product)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", id);
                    cmd.Parameters.AddWithValue("@ProductGenericName", product.GenericName);
                    cmd.Parameters.AddWithValue("@ProductTradeName", product.TradeName);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    cmd.Parameters.AddWithValue("@PresentationId", product.PresentationId);
                    cmd.Parameters.AddWithValue("@ConcentrationId", product.ConcentrationId);
                    cmd.Parameters.AddWithValue("@ConcentrationValue", product.ConcentrationValue); // NUEVO
                    cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId);
                    cmd.Parameters.AddWithValue("@BrandId", product.BrandId);
                    cmd.Parameters.AddWithValue("@IsActive", product.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    Products productUpdate = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productUpdate = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString()!,
                                CategoryId = (int)reader["CategoryId"],
                                PresentationId = (int)reader["PresentationId"],
                                ConcentrationId = (int)reader["ConcentrationId"],
                                ConcentrationValue = reader["ConcentrationValue"].ToString(), // NUEVO
                                SupplierId = (int)reader["SupplierId"],
                                BrandId = (int)reader["BrandId"],
                                IsActive = (bool)reader["IsActive"],
                            };
                        }
                    }

                    return new RepositoryResponse<Products>
                    {
                        Data = productUpdate,
                        OperationStatusCode = 0
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Bloque catch global de resguardo
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Products>> GetByNameAsync(string name)
        {
            var productResult = new Products();
            var response = new RepositoryResponse<Products>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            productResult.ProductId = (int)reader["ProductId"];
                            productResult.GenericName = reader["ProductGenericName"].ToString()!;
                            productResult.TradeName = reader["ProductTradeName"].ToString()!;
                            productResult.oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString()! };
                            productResult.oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString()! };
                            productResult.oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], Volume = reader["Porcentage"].ToString()! };
                            productResult.ConcentrationValue = reader["ConcentrationValue"].ToString(); // NUEVO
                            productResult.oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString()! };
                            productResult.obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString()! };
                            productResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = productResult;
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
            catch (Exception ex) // CORREGIDO: Bloque catch global de resguardo
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<RepositoryResponse<Products>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("UPS_UpdateProductStatus", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Products updatedProduct = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedProduct = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString()!,
                                CategoryId = (int)reader["CategoryId"],
                                PresentationId = (int)reader["PresentationId"],
                                ConcentrationId = (int)reader["ConcentrationId"],
                                ConcentrationValue = reader["ConcentrationValue"].ToString(), // NUEVO
                                SupplierId = (int)reader["SupplierId"],
                                BrandId = (int)reader["BrandId"],
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Products>
                    {
                        Data = updatedProduct,
                        OperationStatusCode = updatedProduct != null ? 0 : 1
                    };
                }
            }
            catch (SqlException ex) // CORREGIDO: Captura específica de código de motor SQL
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<(IEnumerable<Products> Items, int TotalCount)>> GetProductsPagedAsync(int page, int limit)
        {
            var productList = new List<Products>();
            var response = new RepositoryResponse<(IEnumerable<Products> Items, int TotalCount)>();
            int totalRecords = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductsPaged", connection);
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
                            productList.Add(new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString()!,
                                oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString()! },
                                oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString()! },
                                oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], Volume = reader["Porcentage"].ToString()! },
                                ConcentrationValue = reader["ConcentrationValue"].ToString(), // NUEVO
                                oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString()! },
                                obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString()! },
                                IsActive = (bool)reader["IsActive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalRecords = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    response.Data = (productList, totalRecords);
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = (new List<Products>(), 0);
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = (new List<Products>(), 0);
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}