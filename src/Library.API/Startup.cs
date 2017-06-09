﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Models;
using Library.API.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Library.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                // If this is false then the API will return responses in
                // the default format, which is JSON, if an unsupported media type is part
                // of the request.
                setupAction.ReturnHttpNotAcceptable = true;

                // When no Accept header is added to the request, the default formatter chosen is
                // always the first one added to this list.
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Whenever an exception is thrown globally, we'll handle it by 
                // throwing a 500 status and a custom message.
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
            }

            // Install AutoMapper from Nuget and place mappings in the Configure 
            // method of Startup.
            AutoMapper.Mapper.Initialize(config =>
            {
                // Source maps to Destination.
                // Be sure to add projections for special source to destination mappings.
                config.CreateMap<Author, AuthorDto>()
                        .ForMember(dest => dest.Name, option => option.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                        .ForMember(dest => dest.Age, option => option.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

                // No projections necessarily as it's a flat mapping from source to destination.
                config.CreateMap<Book, BookDto>();

                config.CreateMap<AuthorForCreationDto, Author>();

                config.CreateMap<BookForCreationDto, Book>();

                config.CreateMap<BookForUpdateDto, Book>();
            });

            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}
