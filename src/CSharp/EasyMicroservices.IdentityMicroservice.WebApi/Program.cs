using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore.Intrerfaces;
using EasyMicroservices.IdentityMicroservice.BackgroundServices;
using EasyMicroservices.IdentityMicroservice.Database.Contexts;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.IdentityMicroservice.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Serilog;

namespace EasyMicroservices.IdentityMicroservice.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateBuilder(args);
            var build = await app.BuildWithUseCors<IdentityContext>((options) =>
            {

            }, true);

            build.MapControllers();
            //host server need to get token from start
            using var scope = build.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetService<IAppUnitOfWork>();
            await InternalTokenGeneratorBackgroundService.GetToken(unitOfWork);

            await build.RunAsync();
        }

        public static WebApplicationBuilder CreateBuilder(string[] args)
        {
            var app = StartUpExtensions.Create<IdentityContext>(args);
            app.Services.AddLogger((options) =>
            {
                options.UseSerilog(new Serilog.LoggerConfiguration()
                    .WriteTo.File("serilog.txt")
                    .MinimumLevel.Is(Serilog.Events.LogEventLevel.Verbose));
            });

            app.Services.Builder<IdentityContext>("Identity").UseDefaultSwaggerOptions();
            app.Services.AddTransient((serviceProvider) => new UnitOfWork(serviceProvider));
            app.Services.AddTransient(serviceProvider => new IdentityContext(serviceProvider.GetService<IEntityFrameworkCoreDatabaseBuilder>()));
            app.Services.AddTransient<IEntityFrameworkCoreDatabaseBuilder, DatabaseBuilder>();
            app.Services.AddTransient<IAppUnitOfWork, AppUnitOfWork>();
            app.Services.AddTransient((serviceProvider) => new ClaimManager(serviceProvider.GetService<IHttpContextAccessor>()));
            app.Services.AddTransient<IJWTManager, JWTManager>();
            app.Services.AddTransient<IdentityHelper>();
            app.Services.AddHostedService<InternalTokenGeneratorBackgroundService>();
            return app;
        }

        static void AddCors(CorsPolicyBuilder options, params string[] sites)
        {
            options.SetIsOriginAllowed((string origin) =>
               sites.Any(x => new Uri(origin).Host.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }

        public static async Task Run(string[] args, Action<IServiceCollection> use)
        {
            var app = CreateBuilder(args);
            use?.Invoke(app.Services);
            var build = await app.Build<IdentityContext>();
            build.MapControllers();
            await build.RunAsync();
        }
    }
}