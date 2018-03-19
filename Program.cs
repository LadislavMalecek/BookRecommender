using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using BookRecommender.DataManipulation;
using System.Linq;
using System.Reflection;
using System;
using Microsoft.AspNetCore;

namespace BookRecommender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0].ToLower() == "-h" || args[0].ToLower() == "-help"))
            {
                System.Console.WriteLine("Help:");
                System.Console.WriteLine("If you want to run the server, do not use any parameter");
                System.Console.WriteLine("If you want to begin mining the data, use --mine");
                System.Console.WriteLine("For specific mining task use books, authors, characters, genres");
                System.Console.WriteLine("For even more granular usage you can specific which mining task to run");
                System.Console.WriteLine("Books => 0 - 8");
                System.Console.WriteLine("Authors => 0 - 6");
                System.Console.WriteLine("Characters => 0 - 1");
                System.Console.WriteLine("Genres => 0 - 1");
                System.Console.WriteLine("WikiTags => 0 - 1");

            }

            if (args.Length > 0 && args[0].ToLower() == "--mine")
            {
                System.Console.WriteLine("Mining mode active, server will not start");
                DataMiner.Mine(args);
                return;
            }
            if (args.Length > 1 && args[0].ToLower() == "--googleimg")
            {
                var query = string.Join(" ", args.Skip(1));
                var address = new GoogleImageMiner().GetFirstImageUrlAsync(query).Result;
                System.Console.WriteLine(address);
                return;
            }

            if (args.Length > 0)
            {
                System.Console.WriteLine("Params not supported");
                return;
            }


            // Get environment variables
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var insideDocker = config["INSIDE_DOCKER"] == "yes";
            System.Console.WriteLine("Are we inside docker: " + insideDocker);
            string url = insideDocker ? "0.0.0.0" : "*";


            // this code pick the right path to root, because of Linux service does not run with
            // the path 1
            // we try to read the index view from the path location
            string pickedPath = null;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                pickedPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else
            {
                pickedPath = Directory.GetCurrentDirectory();
            }

            BuildWebHost(args, url, pickedPath).Run();
        }

        public static IWebHost BuildWebHost(string[] args, string url, string rootFolder)
        {
            return WebHost.CreateDefaultBuilder(args)
                // .UseKestrel(options =>{
                //     options.UseHttps("C:\\netcore\\myCertificateAuthority\\myCertificates\\10.0.0.10\\10.0.0.10.pfx", "CFahojCFahoj25");
                // })
                .UseKestrel()
                .UseContentRoot(rootFolder)
                .UseIISIntegration()
                .UseStartup<Startup>()
                // .UseUrls($"https://{url}:443")
                .UseUrls($"http://{url}:5000")
                .Build();
        }
        
        static void AppConfiguration(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder){
            var env = webHostBuilderContext.HostingEnvironment;
            
            configurationBuilder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
