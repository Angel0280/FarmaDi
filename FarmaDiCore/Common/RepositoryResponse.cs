using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Common
{
    public class RepositoryResponse<T>
    {
        //Esta propiedad para gestionar (Obtener o asignar) cualquier tipo de dato
        public T? Data { get; set; }

        //Esta propiedad será para gestionar (Obtener o asignar) el código de estado de la operación
        public int OperationStatusCode { get; set; }

        public string? Message { get; set; }

    }
}
