using FinancialReconciliation.Entities;
using FinancialReconciliation.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinancialReconciliation.Services
{
    public class ListOrders
    {
        public string orderNo { get; set; }
    }
    
    public class ShipmentRoot
    {
        public List<ShipmentConfirmArr> shippingConfirmation { get; set; }
    }
    
    public class ShipmentConfirmArr
    {
        public int ID { get; set; }
        public string trackNo { get; set; }
        public int lineNo { get; set; }
        public string orderNo { get; set; }
        public string itemNo { get; set; }
        public int quantity { get; set; }
        public DateTime shipmentDate { get; set; }
    }

    public  class ShipmentProcessing : IShipmentProccessing
    {
        private readonly ILogger<ShipmentProcessing> _logger;

        public ShipmentProcessing(ILogger<ShipmentProcessing> logger)
        {
            _logger = logger;
        }
        public  ShipmentRoot ReadPendingShipments(Parameters parameters)
        {
            try
            {

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.ConnectionString = parameters.connectionString;
                _logger.LogInformation("Query Pending Posting Shipments:");

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    var store = "dbo.Merlin_SP_Qry_ShipConfirm";
                    _logger.LogInformation(store);
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    SqlCommand command = new SqlCommand(store, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    //command.Parameters.AddWithValue("@OrderNumber", "50054791");

                    da = new SqlDataAdapter(command);
                    da.Fill(ds);
                    var dat = ds.Tables[0];
                    List<ShipmentConfirmArr> list = new List<ShipmentConfirmArr>();
                    list = (from DataRow dr in dat.Rows
                            select new ShipmentConfirmArr()
                            {
                                itemNo = getString(dr, "ItemCode"),
                                lineNo = getInt(dr, "LineNumber"),
                                orderNo = getOrder(dr, "OrderNumber"),
                                quantity = getInt(dr, "ShippedQty"),
                                shipmentDate = getDateTime(dr, "ActualShippedDate"),
                                trackNo = getString(dr, "TrackingNumber"),
                                ID = getInt(dr, "id")
                            }).ToList();
                    var data = new ShipmentRoot()
                    {
                        shippingConfirmation = list
                    };
                    _logger.LogInformation("Shipment object obtained");
                    return data;
                }
            }

            catch (SqlException e)
            {
                _logger.LogError(e.ToString());
                return null;
            }

        }

        public async Task RequestShipment(Parameters parameters,List<ShipmentConfirmArr> shipmentConfirmArrs)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            ShipmentRoot root = new ShipmentRoot()
            {
                shippingConfirmation = new List<ShipmentConfirmArr>()
            };

            root.shippingConfirmation.AddRange(shipmentConfirmArrs);

            _logger.LogInformation("Preparing Shipment request");
            Console.WriteLine("Preparing Shipment request");

            Uri uPost = new Uri(parameters.shippingConfirmationEndpoint);
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(root, options);
            Console.WriteLine("jsonBody" + jsonBody);
            _logger.LogInformation("jsonBody" + jsonBody);


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameters.token);
            HttpContent cPost = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage contentPost = await httpClient.PostAsync(uPost, cPost);
            var responseString = await contentPost.Content.ReadAsStringAsync();
            _logger.LogInformation("Response" + responseString);
            Console.WriteLine("Response" + responseString);

            if (contentPost.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successful response");
            }
            else
            {
                _logger.LogInformation("Failed response");
            }
        }

        #region Gets

        private  int getInt(DataRow data, string param)
        {
            if (data[$"{param}"] == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(data[$"{param}"]);
            }
        }

        private  DateTime getDateTime(DataRow data, string param)
        {
            if (data[$"{param}"] == DBNull.Value)
            {
                return DateTime.UtcNow;
            }
            else
            {
                return Convert.ToDateTime(data[$"{param}"]);
            }
        }

        private  string getString(DataRow data, string param)
        {
            if (data[$"{param}"] == DBNull.Value)
            {
                return "";
            }
            else
            {
                return Convert.ToString(data[$"{param}"]);
            }
        }

        private  string getOrder(DataRow data, string param)
        {
            if (data[$"{param}"] == DBNull.Value)
            {
                return "";
            }
            else
            {
                var r = Convert.ToString(data[$"{param}"]);
                return r + "-E10" ;
            }
        }
        #endregion

    }
}
