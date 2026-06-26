using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiCore.Entities.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IKPIDashboardService
    {
        Task<RepositoryResponse<DashboardKpi>> GetDashboardKPIsAsync();
    }
}
