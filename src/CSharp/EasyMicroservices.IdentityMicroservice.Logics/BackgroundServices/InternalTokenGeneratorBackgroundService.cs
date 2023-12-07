using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.BackgroundServices;
public class InternalTokenGeneratorBackgroundService : IHostedService, IDisposable
{
    private Timer _timer = null;
    readonly IAppUnitOfWork _unitOfWork;
    public InternalTokenGeneratorBackgroundService(IAppUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromHours(1));

        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        _ = Task.Run(() => GetToken(_unitOfWork));
    }

    public static async Task GetToken(IAppUnitOfWork appUnitOfWork)
    {
        var logger = appUnitOfWork.GetLogger();
        try
        {
            logger.Debug("Try login...");
            AppUnitOfWork.Token = await appUnitOfWork.GetIdentityHelper().GetFullAccessPersonalAccessToken();
            logger.Debug($"Login success {AppUnitOfWork.Token}");
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}