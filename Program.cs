using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BookRecommender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
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
