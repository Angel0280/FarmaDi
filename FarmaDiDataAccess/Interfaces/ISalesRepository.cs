using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface ISalesRepository
    {
        Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetails> details);
        Task<RepositoryResponse<SaleTransaction>> GetInvoiceByIdAsync(int invoiceId);
        Task<RepositoryResponse<PagedSaleResult>> GetSalesAsync(int pageNumber = 1, int pageSize = 10);
    }
}
