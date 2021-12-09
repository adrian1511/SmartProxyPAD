using Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieAPI.Repositories;
using MovieAPI.Services;
using MovieAPI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace MovieAPI
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
            services.AddHttpContextAccessor();
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbSettings"));
            services.Configure<SyncServiceSettings>(Configuration.GetSection("SyncServiceSettings"));

            services.AddSingleton<IMongoDbSettings>(provider =>
                provider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            services.AddSingleton<ISyncServiceSettings>(provider =>
                provider.GetRequiredService<IOptions<SyncServiceSettings>>().Value);

            services.AddScoped<IMongoRepository<Movie>, MongoRepository<Movie>>();
            services.AddScoped<ISyncService<Movie>, SyncService<Movie>>();

            services.AddCors(option =>
            {
                option.AddPolicy("CorsPolicy",
                    policy => { policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200/"); });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Lab2.Warehouse", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    logger.LogDebug("[{Now}] {Method} {Scheme}://{Host}{Path}",
                        DateTime.Now,
                        context.Request.Method,
                        context.Request.Scheme,
                        context.Request.Host,
                        context.Request.Path);
                    await next.Invoke();
                });
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab2.Warehouse v1"));
            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}