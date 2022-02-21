using FinancialReconciliation.Entities;
using FinancialReconciliation.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialReconciliation.Interfaces
{
    public interface IShipmentProccessing
    {
        ShipmentRoot ReadPendingShipments(Parameters parameters);
        Task RequestShipment(Parameters parameters, List<ShipmentConfirmArr> shipmentConfirmArrs);
    }
}
