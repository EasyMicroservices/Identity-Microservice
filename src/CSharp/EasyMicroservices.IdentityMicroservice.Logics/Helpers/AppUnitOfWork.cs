
using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Clients;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.IdentityMicroservice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

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

        public ClaimManager GetClaimManager()
        {
            return _service.GetService<ClaimManager>();
        }

        string GetValue(string key)
        {
            return GetConfiguration().GetValue<string>(key);
        }

        T SetToken<T>(HttpContext httpContext, T coreSwaggerClient)
            where T : CoreSwaggerClientBase
        {
            if (httpContext.Request.Headers.Authorization.Count > 0)
            {
                coreSwaggerClient.SetBearerToken(httpContext.Request.Headers.Authorization.First());
            }
            return coreSwaggerClient;
        }

        public LanguageClient GetLanguageClient()
        {
            return new LanguageClient(GetValue("RootAddresses:Contents"), new System.Net.Http.HttpClient());
        }

        public UserClient GetUserClient(HttpContext httpContext)
        {
            return SetToken(httpContext, new UserClient(GetValue("RootAddresses:Authentications"), new System.Net.Http.HttpClient()));
        }

        public RoleClient GetRoleClient(HttpContext httpContext)
        {
            return SetToken(httpContext, new RoleClient(GetValue("RootAddresses:Authentications"), new System.Net.Http.HttpClient()));
        }
    }
}
