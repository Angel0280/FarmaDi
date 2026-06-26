using FarmaDi.DataAccess.Repositories;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities.Dashboard;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class KPIDashboardService: IKPIDashboardService
    {
        private readonly IKPIDashboardRepository _dashboardRepository;


        public KPIDashboardService(IKPIDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<RepositoryResponse<DashboardKpi>> GetDashboardKPIsAsync()
        {
            return await _dashboardRepository.GetDashboardKPIsAsync();
        }
    }
}
