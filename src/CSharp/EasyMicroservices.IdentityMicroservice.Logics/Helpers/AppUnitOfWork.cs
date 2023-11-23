
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
using Microsoft.Extensions.DependencyInjection;
using Authentications.GeneratedServices;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class AppUnitOfWork : UnitOfWork, IAppUnitOfWork
    {
        public IServiceProvider _service;
        public AppUnitOfWork(IServiceProvider service) : base(service)
        {
            _service = service;
        }

        public IConfiguration GetConfiguration()
        {
            return _service.GetService<IConfiguration>();
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
