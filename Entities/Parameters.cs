using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialReconciliation.Entities
{
    public class Parameters
    {
        public string connectionString { get; set; }
        public string navEndPointShipments { get; set; }
        public string navEndPointReturns { get; set; }
        public string magentobaseurl { get; set; }
        public string magentouser { get; set; }
        public string magentopassword { get; set; }
        public string shippingConfirmationEndpoint { get; set; }
        public string returnEndpoint { get; set; }
        public string refundEndpoint { get; set; }
        public string token { get; set; }
    }
}
