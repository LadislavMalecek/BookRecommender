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
using BookRecommender.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace BookRecommender
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<BookRecommenderContext>()
                .AddDefaultTokenProviders();

            services.AddResponseCompression();

            services.AddSingleton<IConfiguration>(Configuration);


            // services.Configure<MvcOptions>(options =>
            //     {
            //         options.Filters.Add(new RequireHttpsAttribute());
            //     });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;

                // Cookie settings
                // options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                // options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
                // options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOff";

                // User settings
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.AddSingleton<SpreadingRecommenderCache>();
            services.AddScoped<SpreadingRecommenderEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            BookRecommenderContext db,
            SpreadingRecommenderCache spreadingRecommederCache)
        {

            // add values to the appsetting singleton from appsettings.json
            AppSettingsSingleton.Database.Connection = Configuration["Database:Connection"];
            AppSettingsSingleton.Mining.WikiPagesStorage = Configuration["Mining:WikiPagesStorage"];
            AppSettingsSingleton.Mining.Password = Configuration["Mining:Password"];


            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsProduction())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // var options = new RewriteOptions()
            //     .AddRedirectToHttps();

            app.UseStaticFiles();

            app.UseAuthentication();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseResponseCompression();

            System.Console.WriteLine("Spreading activation cache init started.");
            var sw = Stopwatch.StartNew();
            spreadingRecommederCache.Initialize(db);
            sw.Stop();
            System.Console.WriteLine($"Spreading activation cache initialized, it took: {sw.ElapsedMilliseconds}ms");
        }
    }
}
