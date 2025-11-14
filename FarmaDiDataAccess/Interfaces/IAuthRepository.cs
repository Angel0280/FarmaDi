using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IAuthRepository
    {
        Task<RepositoryResponse<Users>> RegisterAsync(Users user);
        Task<RepositoryResponse<Users>> GetByEmailAsync(string mail);
        Task<RepositoryResponse<Users>> GetByUserNameAsync(string name);
        Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId);

    }
}
