using FinancialReconciliation.Entities;
using FinancialReconciliation.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialReconciliation.Interfaces
{
    public interface IReturnProccessing
    {
        SalesCreditRoot ReadPendingReturns(Parameters parameters);
        Task RequestReturn(Parameters parameters, List<SalesCreditArr> list);
    }
}
