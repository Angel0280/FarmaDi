using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Common
{
    public enum MessageCodes
    {
        Success = 0,
        NotFound = 1,
        ErrorValidation = 2,
        Authentication = 3,
        Unauthorized = 4,
        ErrorDataBase = 5,
        NoData = 6,
        Conflict = 7,
        DuplicateData = 8,
    }
}
