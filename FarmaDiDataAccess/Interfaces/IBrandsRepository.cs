using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IBrandsRepository
    {
        // firma para agregar una marca 
        Task<RepositoryResponse<Brands>> AddAsync(Brands brands);
        // luego que se agrega una marca se prosigue a leerla u obtener todos los registros 
        Task<RepositoryResponse<IEnumerable<Brands>>> GetAllAsync();

        // firma para obtener una categoria por su identificador
        Task<RepositoryResponse<Brands>> GetByIdAsync(int id);

        // firma para actualizar los datos completos de una marca
        Task<RepositoryResponse<Brands>> UpdateAsync(int id, Brands brands);

        // firma para obtener una marca por su nombre
        Task<RepositoryResponse<Brands>> GetByNameAsync(string name);

        // firma para asignar el estado de un registro en catalogo(establecer estado)
        Task<RepositoryResponse<Brands>> SetStateAsync(int id, bool state);
    }
}
