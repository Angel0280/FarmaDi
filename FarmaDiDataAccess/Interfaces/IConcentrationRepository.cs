using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IConcentrationRepository
    {
        // firma para agregar una concentración
      //  Task<RepositoryResponse<Concentrations>> AddAsync(Concentrations concentration);

        // firma para obtener todos los registros 
        Task<RepositoryResponse<IEnumerable<Concentrations>>> GetAllAsync();

        // firma para obtener una concentración por su identificador
       // Task<RepositoryResponse<Concentrations>> GetByIdAsync(int id);

        // firma para actualizar los datos completos de una concentración
       // Task<RepositoryResponse<Concentrations>> UpdateAsync(int id, Concentrations concentration);

        // firma para obtener una concentración por su nombre
       // Task<RepositoryResponse<Concentrations>> GetByNameAsync(string name);

        // firma para asignar el estado de una concentración en catalogo(establecer estado)
      //  Task<RepositoryResponse<Concentrations>> SetStateAsync(int id, bool state);
    }
}