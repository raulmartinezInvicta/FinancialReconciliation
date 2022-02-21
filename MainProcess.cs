using FinancialReconciliation.Entities;
using FinancialReconciliation.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialReconciliation
{
    public class MainProcess
    {
        private readonly ILogger<MainProcess> _logger;
        private readonly IRefundProccessing _refund;
        private readonly IReturnProccessing _return;
        private readonly IShipmentProccessing _shipment;

        public MainProcess(ILogger<MainProcess> logger, IRefundProccessing refund, IReturnProccessing returnt, IShipmentProccessing shipment)
        {
            _logger = logger;
            _refund = refund;
            _return = returnt;
            _shipment = shipment;
        }

        public async Task StartService(Parameters parameters)
        {
            _logger.LogInformation("Main Process exec");

            var shipments = _shipment.ReadPendingShipments(parameters);

            var returns = _return.ReadPendingReturns(parameters);

            var refunds = _refund.ReadPendingRefund(parameters);

            var startTime = Convert.ToDateTime("2021-08-26");

            var endTime = Convert.ToDateTime("2021-12-31");

            var time = startTime;

            do
            {
                var rf = refunds.coRefund.Where(x => x.date >= time && x.date < time.AddDays(1)).ToList();
                if (rf.Count() != 0)
                {
                    _logger.LogInformation("Request Refund");
                    await _refund.RequestRefund(parameters, rf);
                }

                var sh = shipments.shippingConfirmation.Where(x => x.shipmentDate >= time && x.shipmentDate < time.AddDays(1)).ToList();
                if (sh.Count() != 0)
                {
                    _logger.LogInformation("Request Shipment");
                    await _shipment.RequestShipment(parameters, sh);
                }

                var rt = returns.coReturn.Where(x => x.date >= time && x.date < time.AddDays(1)).ToList();
                if (rt.Count() != 0)
                {
                    _logger.LogInformation("Request Return");
                    await _return.RequestReturn(parameters, rt);
                }
                time = time.AddDays(1);
            } while (time >= startTime && time <= endTime);
        }
    }
}
