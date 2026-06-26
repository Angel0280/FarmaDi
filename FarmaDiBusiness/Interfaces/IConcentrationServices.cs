using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IConcentrationServices
    {
        Task<ServiceResponse<IEnumerable<Concentrations>>> GetAllAsync();
    }
}
