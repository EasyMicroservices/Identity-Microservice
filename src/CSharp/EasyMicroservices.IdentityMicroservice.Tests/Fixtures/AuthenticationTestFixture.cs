using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.IdentityMicroservice.BackgroundServices;
using EasyMicroservices.IdentityMicroservice.Database.Contexts;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.IdentityMicroservice.WebApi;
using EasyMicroservices.IdentityMicroservice.WebApi.Controllers;
using Identity.GeneratedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMicroservices.IdentityMicroservice.Tests.Fixtures;
public class AuthenticationTestFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; }
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        var app = Program.CreateBuilder(null);
        string baseUrl = config.GetSection("Urls").Get<string>().Replace("*", "localhost");
        app.Services.AddSingleton(s => new HttpClient());
        app.Services.AddTransient(s => new AuthenticationClient(baseUrl, s.GetService<HttpClient>()));
        app.Services.AddMvc().AddApplicationPart(typeof(AuthenticationController).Assembly);

        var build = await app.Build<IdentityContext>(true);
        build.MapControllers();
        using var scope = build.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetService<IAppUnitOfWork>();
        await InternalTokenGeneratorBackgroundService.GetToken(unitOfWork);
        ServiceProvider = app.Services.BuildServiceProvider();
        _ = build.RunAsync();
    }
}
