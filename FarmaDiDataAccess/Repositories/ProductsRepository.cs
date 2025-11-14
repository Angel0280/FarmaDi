using Azure;
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
    public class ProductsRepository:IProductsRepository
    {


        private readonly string _connectionString;
        public ProductsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Products>> AddAsync(Products Product)
        {
            var response = new Products();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProductTradeName", Product.TradeName);
                    cmd.Parameters.AddWithValue("@ProductGenericName", Product.GenericName);
                    cmd.Parameters.AddWithValue("@CategoryId", Product.CategoryId);
                    cmd.Parameters.AddWithValue("@PresentationId", Product.PresentationId);
                    cmd.Parameters.AddWithValue("@ConcentrationId", Product.ConcentrationId);
                    cmd.Parameters.AddWithValue("@SupplierId", Product.SupplierId);
                    cmd.Parameters.AddWithValue("@BrandId", Product.BrandId);
                    //cmd.Parameters.AddWithValue("@IsActive", Product.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.ProductId = (int)reader["ProductId"];
                            response.TradeName = reader["ProductTradeName"].ToString()!;
                            response.GenericName = reader["ProductGenericName"].ToString();
                            response.CategoryId =(int) reader["CategoryId"];
                           
                            response.PresentationId =(int) reader["PresentationId"];
                            response.ConcentrationId = (int)reader["ConcentrationId"];
                            response.SupplierId = (int)reader["SupplierId"];
                            response.BrandId = (int)reader["BrandId"];
                            
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }


                    // capturamos el código que viene del procedimiento 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Products>
                    {
                        Data = response,
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
        }


   
        public async Task<RepositoryResponse<IEnumerable<Products>>> GetAllAsync()
        {
            var Products = new List<Products>();
            var response = new RepositoryResponse<IEnumerable<Products>>();
            try
            {
                // establecemos la conexion con la base de datos
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
                            Products.Add(new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString(),
                                oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString()},  
                                oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString()},
                                oconcentration  = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], ConcentrationName = reader["Porcentage"].ToString() },
                                oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"],SupplierName = reader["SupplierName"].ToString() } ,
                                obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString() },
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }
                    //Capturando el valor que retorna  el procedimiento almacenado 
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = Products;
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
            var response = new Products();
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
                            response.ProductId = (int)reader["ProductId"];
                            response.GenericName = reader["ProductGenericName"].ToString()!;
                            response.TradeName = reader["ProductTradeName"].ToString();
                            response.oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString() };
                            response.oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString() };
                            response.oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], ConcentrationName = reader["Porcentage"].ToString() };
                            response.oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString() };
                            response.obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString() };
                            response.IsActive = (bool)reader["IsActive"];

                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);


                    return new RepositoryResponse<Products>
                    {
                        Data = response,
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

        }


        public async Task<RepositoryResponse<Products>> UpdateAsync(int id, Products Products)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateProduct", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", id);
                    cmd.Parameters.AddWithValue("@ProductGenericName", Products.GenericName);
                    cmd.Parameters.AddWithValue("@ProductTradeName", Products.TradeName);
                    cmd.Parameters.AddWithValue("@CategoryId", Products.CategoryId);
            
                    cmd.Parameters.AddWithValue("@PresentationId", Products.PresentationId);
                    cmd.Parameters.AddWithValue("@ConcentrationId", Products.ConcentrationId);
                    cmd.Parameters.AddWithValue("@SupplierId", Products.SupplierId);
                    cmd.Parameters.AddWithValue("@BrandId", Products.BrandId);
                 
                    cmd.Parameters.AddWithValue("@IsActive", Products.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;


                     Products ProductUpdate = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ProductUpdate = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString(),
                                oCategory = new Categories { CategoryId = (int)reader["CategoryId"] },
                                oPresentation = new Presentations { Id = (int)reader["PresentationId"] } ,
                                oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"] },
                                oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"] },
                                obrand = new Brands { BrandId = (int)reader["BrandId"] },
                                IsActive = (bool)reader["IsActive"],

                            };
                        }
                    }
                    // var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    // response.Data = ProductUpdate;
                    // response.OperationStatusCode = returnedValue;

                    return new RepositoryResponse<Products>
                    {
                        Data = ProductUpdate,
                        OperationStatusCode = 0,


                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Products>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message,

                };
            }
        }



        public async Task<RepositoryResponse<Products>> GetByNameAsync(string name)
        {
            var Product = new Products();
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

                            Product.ProductId = (int)reader["ProductId"];
                            Product.GenericName = reader["ProductGenericName"].ToString()!;
                            Product.TradeName = reader["ProductTradeName"].ToString();
                            Product.oCategory = new Categories { CategoryId = (int)reader["CategoryId"], CategoryName = reader["CategoryName"].ToString() };
                            Product.oPresentation = new Presentations { Id = (int)reader["PresentationId"], Description = reader["PresentationDescription"].ToString() };
                            Product.oconcentration = new Concentrations { ConcentrationId = (int)reader["ConcentrationId"], ConcentrationName = reader["Porcentage"].ToString() };
                            Product.oSupplier = new Suppliers { SupplierId = (int)reader["SupplierId"], SupplierName = reader["SupplierName"].ToString() };
                            Product.obrand = new Brands { BrandId = (int)reader["BrandId"], BrandName = reader["BrandName"].ToString() };
                            Product.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = Product;
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

                    Products Updated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Updated = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString(),
                                TradeName = reader["ProductTradeName"].ToString(),
                                CategoryId =(int) reader["CategoryId"],
                               
                                PresentationId = (int) reader["PresentationId"],
                                ConcentrationId = (int) reader["ConcentrationId"],
                                SupplierId = (int) reader["SupplierId"],
                                BrandId = (int) reader["BrandId"],

                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Products>
                    {
                        Data = Updated,
                        OperationStatusCode = Updated != null ? 0 : 1
                    };
                }
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


    }
}
