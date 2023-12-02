using EasyMicroservices.IdentityMicroservice.Database.Contexts;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore.Intrerfaces;
using EasyMicroservices.IdentityMicroservice;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.IdentityMicroservice.Helpers;
using Microsoft.OpenApi.Models;

namespace EasyMicroservices.IdentityMicroservice.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateBuilder(args);
            var build = await app.Build<IdentityContext>(true);
            build.MapControllers();
            build.Run();
        }

        static WebApplicationBuilder CreateBuilder(string[] args)
        {
            var app = StartUpExtensions.Create<IdentityContext>(args);
            app.Services.Builder<IdentityContext>(options =>
            {
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            //Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            }).UseDefaultSwaggerOptions();
            app.Services.AddTransient((serviceProvider) => new UnitOfWork(serviceProvider));
            app.Services.AddTransient(serviceProvider => new IdentityContext(serviceProvider.GetService<IEntityFrameworkCoreDatabaseBuilder>()));
            app.Services.AddTransient<IEntityFrameworkCoreDatabaseBuilder, DatabaseBuilder>();
            app.Services.AddTransient<IAppUnitOfWork, AppUnitOfWork>();
            app.Services.AddTransient((serviceProvider) => new ClaimManager(serviceProvider.GetService<IHttpContextAccessor>()));
            StartUpExtensions.AddWhiteLabel("Identity", "RootAddresses:WhiteLabel");
            return app;
        }

        public static async Task Run(string[] args, Action<IServiceCollection> use)
        {
            var app = CreateBuilder(args);
            use?.Invoke(app.Services);
            var build = await app.Build<IdentityContext>();
            build.MapControllers();
            build.Run();
        }
    }
}