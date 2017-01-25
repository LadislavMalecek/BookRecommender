using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BookRecommender.DataManipulation;

namespace BookRecommender
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            //services.AddDbContext<BookRecommenderContext>(options => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BookRecommender;Integrated Security=True;Connect Timeout=30;"));
            // services.AddDbContext<BookRecommenderContext>(options => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BookRecommender"));
            //Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
            
            //SQLite
            services.AddDbContext<BookRecommenderContext>();
     
     }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


            // new Author{ 
            //         URI = "blabla",
            //         Name = "Evzen",
            //         DateBirth = new DateTime(1990,10,10),
            //         DateDeath = DateTime.Now
            // }
            // .AddToDb();


            // new Author{ 
            //         URI = "www",
            //         Name = "AWEFAWEF",
            //         DateBirth = new DateTime(1680,10,10),
            //         DateDeath = DateTime.Now
            // }
            // .AddToDb();

            // new Author{ 
            //         URI = "blabla.com",
            //         Name = "SERG",
            //         DateBirth = new DateTime(1998,10,10),
            //         NameEn= "pussy"
            // }
            // .AddToDb();



            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
