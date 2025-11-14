using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface ICategoriesRepository
    {
        // firma para agregar una categoria 
        Task<RepositoryResponse<Categories>> AddAsync(Categories categories);
        // luego que se agrega una categoria se prosigue a leerla u obtener todos los registros 
        Task<RepositoryResponse<IEnumerable<Categories>>> GetAllAsync();

        // firma para obtener una categoria por su identificador
        Task<RepositoryResponse<Categories>> GetByIdAsync(int id);

        // firma para actualizar los datos completos de una categoria
        Task<RepositoryResponse<Categories>> UpdateAsync(int id, Categories categories);

        // firma para obtener una categoria por su nombre
        Task<RepositoryResponse<Categories>> GetByNameAsync(string name);

        // firma para asignar el estado de un registro en catalogo(establecer estado)
        Task<RepositoryResponse<Categories>> SetStateAsync(int id, bool state);
    }
}
