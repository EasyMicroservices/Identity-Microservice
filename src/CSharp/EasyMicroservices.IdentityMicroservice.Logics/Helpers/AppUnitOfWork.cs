
using Microsoft.Extensions.Configuration;
using System;
using EasyMicroservices.IdentityMicroservice;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.Xml;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Services;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class AppUnitOfWork : UnitOfWork, IAppUnitOfWork
    {
        public AppUnitOfWork(IServiceProvider service) : base(service)
        {
        }

        public IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        }

        public IdentityHelper GetIdentityHelper()
        {
            return new IdentityHelper(GetConfiguration(), GetIJWTManager());
        }

        public IJWTManager GetIJWTManager()
        {
            return new JWTManager(GetConfiguration());
        }
    }
}
