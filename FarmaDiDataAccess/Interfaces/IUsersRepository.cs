using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IUsersRepository
    {
        Task<RepositoryResponse<Users>> AddAsync(Users users);
        Task<RepositoryResponse<IEnumerable<Users>>> GetAllAsync();
        Task<RepositoryResponse<Users>> GetByIdAsync(int id);
        Task<RepositoryResponse<Users>> UpdateAsync(int id, Users users);
        Task<RepositoryResponse<Users>> GetByNameAsync(string name);
        Task<RepositoryResponse<Users>> GetByEmailAsync(string email);
        Task<RepositoryResponse<Users>> SetStateAsync(int id, bool state);

        Task<RepositoryResponse<RolesUers>> RegisterUserWithRolesAsync(Users user, IEnumerable<Roles> roleIds);

    }
}
