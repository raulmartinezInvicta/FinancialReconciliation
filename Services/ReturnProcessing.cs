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
    public class SalesCreditArr
    {
        public string orderNo { get; set; }
        public string lineNo { get; set; }
        public string lineType { get; set; }
        public DateTime date { get; set; }
        public string number { get; set; }
        public string description { get; set; }
        public string quantity { get; set; }
        public string amount { get; set; }
        public string returnShipment { get; set; }
        public string returnComments { get; set; }
        public string returnNo { get; set; }
    }

    public class SalesCreditArrProt
    {
        public string orderNo { get; set; }
        public string lineNo { get; set; }
        public DateTime date { get; set; }
        public string number { get; set; }
        public string qty_ordered { get; set; }
        public string qty_shipped { get; set; }
        public string qty_refunded { get; set; }
        public string qty_cancelled { get; set; }
        public string description { get; set; }
        public string amount { get; set; }
        public string returnShipment { get; set; }
        public string returnComments { get; set; }
        public string returnNo { get; set; }
    }

    public class SalesCreditRoot
    {
        public List<SalesCreditArr> coReturn { get; set; }
    }

    public class ReturnProcessing : IReturnProccessing
    {
        private readonly ILogger<ReturnProcessing> _logger;

        public ReturnProcessing(ILogger<ReturnProcessing> logger)
        {
            _logger = logger;
        }

        public  SalesCreditRoot ReadPendingReturns(Parameters parameters)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.ConnectionString = parameters.connectionString;
            _logger.LogInformation("Query Return Process:");
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    var store = "dbo.Merlin_SP_Qry_Return";
                    _logger.LogInformation(store);
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    SqlCommand command = new SqlCommand(store, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    da = new SqlDataAdapter(command);
                    da.Fill(ds);
                    var dat = ds.Tables[0];
                    List<SalesCreditArrProt> origin = new List<SalesCreditArrProt>();
                    origin = (from DataRow dr in dat.Rows
                            select new SalesCreditArrProt()
                            {
                                orderNo = getOrder(dr, "orderNo"),
                                lineNo = getString(dr, "lineNo"),
                                qty_ordered = getQty(dr, "qty_ordered"),
                                qty_shipped = getQty(dr, "qty_shipped"),
                                qty_refunded = getQty(dr, "qty_refunded"),
                                qty_cancelled = getQty(dr, "qty_cancelled"),
                                date = getDateTime(dr, "date"),
                                number = getString(dr, "number"),
                                description = getString(dr, "description"),
                                amount = "0",
                                returnShipment = getShipment(dr, "returnShipment"),
                                returnComments = getString(dr, "number"),
                                returnNo = getString(dr, "returnNo")
                            }).ToList();

                    List<SalesCreditArr> list = new List<SalesCreditArr>();

                    foreach(SalesCreditArrProt scp in origin)
                    {
                        if(Convert.ToDouble(scp.qty_refunded) == Convert.ToDouble(scp.qty_cancelled))
                        {
                            var itemList = new SalesCreditArr()
                            {
                                orderNo = scp.orderNo,
                                lineNo = scp.lineNo,
                                lineType = "0",
                                date = scp.date,
                                amount = "0",
                                description = scp.description,
                                number = scp.number,
                                quantity = scp.qty_cancelled,
                                returnComments = scp.returnComments,
                                returnNo = scp.returnNo,
                                returnShipment = "0"
                            };
                            list.Add(itemList);

                        }
                        else if(Convert.ToDouble(scp.qty_refunded) > Convert.ToDouble(scp.qty_cancelled))
                        {
                            var realRefund = Convert.ToDouble(scp.qty_refunded) - Convert.ToDouble(scp.qty_cancelled);
                            if (Convert.ToDouble(scp.qty_cancelled) != 0)
                            {
                                var itemList = new SalesCreditArr()
                                {
                                    orderNo = scp.orderNo,
                                    lineNo = scp.lineNo,
                                    lineType = "0",
                                    date = scp.date,
                                    amount = "0",
                                    description = scp.description,
                                    number = scp.number,
                                    quantity = scp.qty_cancelled,
                                    returnComments = scp.returnComments,
                                    returnNo = scp.returnNo,
                                    returnShipment = "0"
                                };
                                var itemList2 = new SalesCreditArr()
                                {
                                    orderNo = scp.orderNo,
                                    lineNo = scp.lineNo,
                                    lineType = "0",
                                    date = scp.date,
                                    amount = "0",
                                    description = scp.description,
                                    number = scp.number,
                                    quantity = realRefund.ToString(),
                                    returnComments = scp.returnComments,
                                    returnNo = scp.returnNo,
                                    returnShipment = "1"
                                };

                                list.Add(itemList);
                                list.Add(itemList2);
                            }
                            else
                            {
                                var itemList = new SalesCreditArr()
                                {
                                    orderNo = scp.orderNo,
                                    lineNo = scp.lineNo,
                                    lineType = "0",
                                    date = scp.date,
                                    amount = "0",
                                    description = scp.description,
                                    number = scp.number,
                                    quantity = scp.qty_refunded,
                                    returnComments = scp.returnComments,
                                    returnNo = scp.returnNo,
                                    returnShipment = scp.returnShipment
                                };
                                list.Add(itemList);
                            }
                           
                        }
                        else
                        {
                            var itemList = new SalesCreditArr()
                            {
                                orderNo = scp.orderNo,
                                lineNo = scp.lineNo,
                                lineType = "0",
                                date = scp.date,
                                amount = "0",
                                description = scp.description,
                                number = scp.number,
                                quantity = scp.qty_refunded,
                                returnComments = scp.returnComments,
                                returnNo = scp.returnNo,
                                returnShipment = scp.returnShipment
                            };
                            list.Add(itemList);
                        }
                    }

                    List<SalesCreditArr> list2 = new List<SalesCreditArr>();
                    list2 = (from DataRow dr in dat.Rows
                             select new SalesCreditArr()
                             {
                                 orderNo = getOrder(dr, "orderNo"),
                                 lineNo = getString(dr, "lineNo"),
                                 lineType = "1",
                                 date = getDateTime(dr, "date"),
                                 number = "41660",
                                 quantity = getString(dr, "qty_refunded"),
                                 description = "adjustment refund",
                                 amount = getString(dr, "adjustmentRefund"),
                                 returnShipment = getShipment(dr, "returnShipment"),
                                 returnComments = getString(dr, "number"),
                                 returnNo = getString(dr, "returnNo")
                             }).ToList();

                    List<SalesCreditArr> list3 = new List<SalesCreditArr>();
                    list3 = (from DataRow dr in dat.Rows
                             select new SalesCreditArr()
                             {
                                 orderNo = getOrder(dr, "orderNo"),
                                 lineNo = getString(dr, "lineNo"),
                                 lineType = "1",
                                 date = getDateTime(dr, "date"),
                                 number = "41640",
                                 quantity = getString(dr, "qty_refunded"),
                                 description = "adjustment refund for shipping",
                                 amount = Convert.ToString(getDouble(dr, "shipping_refunded") + getDouble(dr, "shipping_tax_refunded")),
                                 returnShipment = getShipment(dr, "returnShipment"),
                                 returnComments = getString(dr, "number"),
                                 returnNo = getString(dr, "returnNo")
                             }).ToList();

                    List<SalesCreditArr> list4 = new List<SalesCreditArr>();
                    list4 = (from DataRow dr in dat.Rows
                             select new SalesCreditArr()
                             {
                                 orderNo = getOrder(dr, "orderNo"),
                                 lineNo = getString(dr, "lineNo"),
                                 lineType = "1",
                                 date = getDateTime(dr, "date"),
                                 number = "41651",
                                 quantity = getString(dr, "qty_refunded"),
                                 description = "adjustment fee / Tax fee",
                                 amount = getString(dr, "adjustmentFee"),
                                 returnShipment = getShipment(dr, "returnShipment"),
                                 returnComments = getString(dr, "number"),
                                 returnNo = getString(dr, "returnNo")
                             }).ToList();

                    foreach (SalesCreditArr l4 in list4)
                    {
                        if (Convert.ToDouble(l4.amount) == 7.5)
                        {
                            l4.number = "41641";
                            l4.description = "adjustment fee / Restocking fee";
                        }
                    }

                    var nl2 = new List<SalesCreditArr>();
                    foreach (SalesCreditArr sl2 in list2)
                    {

                        var d = nl2.Find(x => x.orderNo == sl2.orderNo && x.returnNo == sl2.returnNo);
                        if(d == null)
                        {
                            nl2.Add(sl2);
                        }
                    }

                    var nl4 = new List<SalesCreditArr>();
                    foreach (SalesCreditArr sl4 in list4)
                    {
                        var d = nl4.FirstOrDefault(x => x.orderNo == sl4.orderNo && x.returnNo == sl4.returnNo);
                        if (d == null)
                        {
                            nl4.Add(sl4);
                        }
                    }

                    list.AddRange(nl2);
                    list.AddRange(list3);
                    list.AddRange(nl4);

                    var nl = new List<SalesCreditArr>();
                    foreach (SalesCreditArr sl in list)
                    {
                        
                        if (sl.lineType == "1" && Convert.ToDouble(sl.amount) != 0)
                        {
                            nl.Add(sl);
                        }
                        else if(sl.lineType == "0")
                        {
                            nl.Add(sl);
                        }
                        
                    }

                    var data = new SalesCreditRoot()
                    {
                        coReturn = nl
                    };
                    _logger.LogInformation("Return object obtained");
                    return (data);
                }  
            }
            catch (Exception e)
            {
                
                _logger.LogError(e.ToString());
                return null;
            }
        }

        public  async Task RequestReturn(Parameters parameters,List<SalesCreditArr> list)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var root = new SalesCreditRoot()
            {
                coReturn = new List<SalesCreditArr>()
            };
            root.coReturn.AddRange(list);
            Uri uPost = new Uri(parameters.returnEndpoint);
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

            private  string getQty(DataRow data, string param)
            {
                if (data[$"{param}"] == DBNull.Value)
                {
                    return "0";
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

