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
    public class Refund
    {
        public List<RefundLines> coRefund { get; set; }
    }

    public class RefundLines
    {
        public string orderNo { get; set; }
        public DateTime date { get; set; }
        public string refundAmount { get; set; }
        public string refundCode { get; set; }
        public string refundNo { get; set; }
    }

    public class RefundLinesValidated
    {
        public string orderNo { get; set; }
        public DateTime date { get; set; }
        public string refundAmount { get; set; }
        public string refundCode { get; set; }
        public string refundNo { get; set; }
        public string isCouponRefound { get; set; }
    }
    
    public class RefundProcessing : IRefundProccessing
    {
        private readonly ILogger<RefundProcessing> _logger;

        public RefundProcessing(ILogger<RefundProcessing> logger)
        {
            _logger = logger;
        }
        public Refund ReadPendingRefund(Parameters parameters)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = parameters.connectionString;
            _logger.LogInformation("Query Refund Process:");

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    var store = "dbo.Merlin_SP_Qry_Refund";
                    _logger.LogInformation(store);
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    SqlCommand command = new SqlCommand(store, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    da = new SqlDataAdapter(command);
                    da.Fill(ds);
                    var dat = ds.Tables[0];
                    List<RefundLinesValidated> list = new List<RefundLinesValidated>();
                    list = (from DataRow dr in dat.Rows
                            select new RefundLinesValidated()
                            {
                                orderNo = getOrder(dr, "increment_id"),
                                date = getDateTime(dr, "updated_at"),
                                isCouponRefound = getString(dr, "isCouponRefund"),
                                refundAmount = getString(dr, "refundAmount"),
                                refundCode = getString(dr, "refundCode"),
                                refundNo = getString(dr, "refundNo")
                            }).ToList();

                    _logger.LogInformation("Refund object obtained");
                    List<RefundLines> response = new List<RefundLines>();
                    foreach (RefundLinesValidated r in list)
                    {
                        if(r.isCouponRefound == "1")
                        {
                            var itemResponse = new RefundLines()
                            {
                                refundAmount = r.refundAmount,
                                date = r.date,
                                orderNo = r.orderNo,
                                refundCode = r.refundCode,
                                refundNo = r.refundNo
                            };
                            response.Add(itemResponse);
                        }
                    }
                    var data = new Refund()
                    {
                        coRefund = response
                    };

                    return (data);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }
        }

        public async Task RequestRefund(Parameters parameters, List<RefundLines> list)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var root = new Refund()
            {
                coRefund = new List<RefundLines>()
            };
            root.coRefund.AddRange(list);
            Uri uPost = new Uri(parameters.refundEndpoint);
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(root, options);
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
                Console.WriteLine("Successful response");
            }
        }

        #region Gets



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

        private  double getDouble(DataRow data, string param)
        {
            if (data[$"{param}"] == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToDouble(data[$"{param}"]);
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
                return r + "-E10";
            }
        }

        private  string getShipment(DataRow data, string param)
        {
            var shipment = Convert.ToString(data[$"{param}"]);
            if (shipment == "0")
            {
                return "false";
            }
            else
            {
                return "true";
            }
        }
        #endregion
    }
}
