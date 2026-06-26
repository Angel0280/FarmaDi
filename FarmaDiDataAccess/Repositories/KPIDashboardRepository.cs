using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiCore.Entities.Dashboard;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDi.DataAccess.Repositories
{
    public class DashboardRepository : IKPIDashboardRepository
    {
        private readonly string _dwConnectionString;

        public DashboardRepository(IConfiguration configuration)
        {
            _dwConnectionString = configuration.GetConnectionString("DataWarehouseConnection");
        }

        public async Task<RepositoryResponse<DashboardKpi>> GetDashboardKPIsAsync()
        {
            var response = new RepositoryResponse<DashboardKpi>();
            var dashboard = new DashboardKpi();

            try
            {
                using var connection = new SqlConnection(_dwConnectionString);
                using var command = new SqlCommand("usp_GetDashboardKPIs", connection);

                command.CommandType = CommandType.StoredProcedure;

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                // 1. Evolución de ventas por periodo
                while (await reader.ReadAsync())
                {
                    dashboard.SalesByPeriod.Add(new SalesByPeriod
                    {
                        Periodo = reader["Periodo"].ToString()!,
                        TotalVentas = Convert.ToDecimal(reader["TotalVentas"])
                    });
                }

                // 2. Top productos por ingresos
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dashboard.TopProductRevenues.Add(new TopProductRevenue
                        {
                            Producto = reader["Producto"].ToString()!,
                            TotalIngresos = Convert.ToDecimal(reader["TotalIngresos"])
                        });
                    }
                }

                // 3. Top productos más vendidos
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dashboard.TopProductsSold.Add(new TopProductSold
                        {
                            Producto = reader["Producto"].ToString()!,
                            CantidadVendida = Convert.ToInt32(reader["CantidadVendida"])
                        });
                    }
                }

                // 4. Ventas por trimestre
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dashboard.QuarterSales.Add(new QuarterSales
                        {
                            Trimestre = reader["Trimestre"].ToString()!,
                            TotalVentas = Convert.ToDecimal(reader["TotalVentas"])
                        });
                    }
                }

                // 5. Crecimiento mensual
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dashboard.MonthlyGrowth.Add(new MonthlyGrowth
                        {
                            Periodo = reader["Periodo"].ToString()!,
                            TotalVentas = Convert.ToDecimal(reader["TotalVentas"]),
                            VentasMesAnterior = reader["VentasMesAnterior"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["VentasMesAnterior"]),
                            PorcentajeCrecimiento = reader["PorcentajeCrecimiento"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["PorcentajeCrecimiento"])
                        });
                    }
                }

                response.Data = dashboard;
                response.Message = "Datos del dashboard obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                response.Message = $"Error obteniendo KPIs del dashboard: {ex.Message}";
            }

            return response;
        }
    }
}