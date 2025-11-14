using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IPresentationRepository
    {
        // firma para agregar una presentación
        Task<RepositoryResponse<Presentations>> AddAsync(Presentations presentation);
        // luego que se agrega una presentación se prosigue a leerla u obtener todos los registros 
        Task<RepositoryResponse<IEnumerable<Presentations>>> GetAllAsync();

        // firma para obtener una presentación por su identificador
        Task<RepositoryResponse<Presentations>> GetByIdAsync(int id);

        // firma para actualizar los datos completos de una presentación
        Task<RepositoryResponse<Presentations>> UpdateAsync(int id, Presentations presentation);

        // firma para obtener una presentación por su nombre
        Task<RepositoryResponse<Presentations>> GetByNameAsync(string name);

        // firma para asignar el estado de un registro en catalogo(establecer estado)
        Task<RepositoryResponse<Presentations>> SetStateAsync(int id, bool state);
    }
}
