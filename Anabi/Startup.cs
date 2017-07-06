﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Anabi.DataAccess.Abstractions.Repositories;
using Anabi.DataAccess.Repositories;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Anabi.DataAccess.Ef;
using Microsoft.EntityFrameworkCore;
using Anabi.DataAccess.Ef.DbModels;

namespace Anabi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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

            AddDbContext(services);

            MapInterfacesAndClasses(services);            
        }

        private void MapInterfacesAndClasses(IServiceCollection services)
        {
            services.AddScoped<IGenericRepository<CategoryDb>, CategoriesRepository>();
            //services.AddScoped<ICategoriesRepository, CategoriesRepository>();
            //services.AddScoped<IInculpatiRepository, InculpatiRepository>();
            //services.AddScoped<IBunuriRepository, BunuriRepository>();
            //services.AddScoped<IDosareRepository, DosareRepository>();
            //services.AddScoped<IJudetRepository, JudetRepository>();

        }

        private void AddDbContext(IServiceCollection services)
        {

            var connection = Configuration.GetConnectionString("AnabiDatabase");
            services.AddDbContext<AnabiContext>(options =>
            options.UseSqlServer(connection,
                         sqlServerOptionsAction: sqlOptions =>
                         {
                             sqlOptions.EnableRetryOnFailure(maxRetryCount: 5,

                             maxRetryDelay: TimeSpan.FromSeconds(15),

                             errorNumbersToAdd: null);

                         }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());


            app.UseMvc();

            var context = app.ApplicationServices.GetService<AnabiContext>();
            DbInitializer.Initialize(context);
        }
    }
}