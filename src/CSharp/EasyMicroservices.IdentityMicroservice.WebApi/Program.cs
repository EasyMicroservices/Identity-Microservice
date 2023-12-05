using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore.Intrerfaces;
using EasyMicroservices.IdentityMicroservice.BackgroundServices;
using EasyMicroservices.IdentityMicroservice.Database.Contexts;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.IdentityMicroservice.Services;
using EasyMicroservices.Logger.Interfaces;
using EasyMicroservices.Logger.Options;
using EasyMicroservices.Logger.Serilog.Providers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EasyMicroservices.IdentityMicroservice.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateBuilder(args);
            var build = await app.Build<IdentityContext>(true);
            build.MapControllers();
            var scope = build.Services.CreateScope();
            await build.RunAsync();
        }

        static WebApplicationBuilder CreateBuilder(string[] args)
        {
            var app = StartUpExtensions.Create<IdentityContext>(args);
            app.Services.AddLogger((options) =>
            {
                options.UseSerilog(new Serilog.LoggerConfiguration()
                    .WriteTo.File("serilog.txt")
                    .MinimumLevel.Is(Serilog.Events.LogEventLevel.Verbose));
            });
            app.Services.Builder<IdentityContext>().UseDefaultSwaggerOptions();
            app.Services.AddTransient((serviceProvider) => new UnitOfWork(serviceProvider));
            app.Services.AddTransient(serviceProvider => new IdentityContext(serviceProvider.GetService<IEntityFrameworkCoreDatabaseBuilder>()));
            app.Services.AddTransient<IEntityFrameworkCoreDatabaseBuilder, DatabaseBuilder>();
            app.Services.AddTransient<IAppUnitOfWork, AppUnitOfWork>();
            app.Services.AddTransient((serviceProvider) => new ClaimManager(serviceProvider.GetService<IHttpContextAccessor>()));
            app.Services.AddTransient<IJWTManager, JWTManager>();
            app.Services.AddTransient<IdentityHelper>();
            app.Services.AddHostedService<InternalTokenGeneratorBackgroundService>();
            StartUpExtensions.AddWhiteLabel("Identity", "RootAddresses:WhiteLabel");
            return app;
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