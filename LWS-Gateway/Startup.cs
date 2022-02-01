using System.Diagnostics.CodeAnalysis;
using LWS_Gateway.Configuration;
using LWS_Gateway.Filter;
using LWS_Gateway.Kube;
using LWS_Gateway.Middleware;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using LWS_Gateway.Swagger;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace LWS_Gateway
{
    [ExcludeFromCodeCoverage]
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
            // MongoDB Configuration
            services.AddSingleton(Configuration.GetSection("MongoConfiguration").Get<MongoConfiguration>());
            
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => { options.LoginPath = "/account/login"; });
            
            services.AddSingleton<MongoContext>();
            services.AddSingleton<IAccountRepository, AccountRepository>();
            services.AddSingleton<AuthenticationService>();
            services.AddScoped<UserService>();
            services.AddScoped<IKubernetesService, KubernetesService>();
            services.AddSingleton<ServiceDeploymentProvider>();
            services.AddSingleton<IDeploymentRepository, DeploymentRepository>();

            services.AddControllersWithViews();
            services.AddControllers(option => option.Filters.Add<CustomExceptionFilter>());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "LWS_Gateway", Version = "v1"});
                c.OperationFilter<SwaggerHeaderOptions>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LWS_Gateway v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict
            });
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}