using FinancialReconciliation.Entities;
using FinancialReconciliation.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialReconciliation.Interfaces
{
    public interface IRefundProccessing
    {
        Refund ReadPendingRefund(Parameters parameters);
        Task RequestRefund(Parameters parameters, List<RefundLines> list);
    }
}
