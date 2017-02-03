using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using BookRecommender.DataManipulation;

namespace BookRecommender
{
    public class Program
    {
        public static void Main(string[] args)
        {

            if(args.Length > 0 && args[0].ToLower() == "--mine"){
                System.Console.WriteLine("Mining mode active, server will not start");
                MineSPARQL.Mine(args);
                return;
            }

            if(args.Length > 0){
                System.Console.WriteLine("Params not supported");
                return;
            }
            
            // var config = new ConfigurationBuilder()
            //     .AddCommandLine(args)
            //     .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            //     .Build();

            // Get environment variables
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
                
            var insideDocker = config["INSIDE_DOCKER"] == "yes";
            System.Console.WriteLine("Are we inside docker: " + insideDocker);
            string url = insideDocker ? "0.0.0.0" : "localhost";

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls($"http://{url}:5000")
                // .UseUrls("http://0.0.0.0:5000")
                .Build();

            host.Run();
        }
    }
}
