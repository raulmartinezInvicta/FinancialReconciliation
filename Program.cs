using FinancialReconciliation.Entities;
using FinancialReconciliation.Interfaces;
using FinancialReconciliation.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinancialReconciliation
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
          .WriteTo.File("FinancialReconciliation.log", rollingInterval: RollingInterval.Day)
          .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonParameters = File.ReadAllText("appsettings.json");
            var jsonParamModel = JsonSerializer.Deserialize<Parameters>(jsonParameters, options);
            await serviceProvider.GetService<MainProcess>().StartService(jsonParamModel);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog())
                    .AddTransient<MainProcess>()
                    .AddTransient<IRefundProccessing,RefundProcessing>()
                    .AddTransient<IReturnProccessing,ReturnProcessing>()
                    .AddTransient<IShipmentProccessing,ShipmentProcessing>();
        }
    }
}
