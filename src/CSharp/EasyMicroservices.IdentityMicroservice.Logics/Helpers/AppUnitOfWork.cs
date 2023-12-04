
using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Clients;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.Logger.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Text;

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
            return _service.GetService<IdentityHelper>();
        }

        public IJWTManager GetIJWTManager()
        {
            return _service.GetService<IJWTManager>();
        }

        public ClaimManager GetClaimManager()
        {
            return _service.GetService<ClaimManager>();
        }

        string GetValue(string key)
        {
            return GetConfiguration().GetValue<string>(key);
        }

        public IHttpContextAccessor GetHttpContextAccessor()
        {
            return _service.GetService<IHttpContextAccessor>();
        }

        static HttpClient CurrentHttpClient { get; set; } = new HttpClient();

        public static string Token = "";
        T InternalLogin<T>(T client)
            where T : CoreSwaggerClientBase
        {
            client.SetBearerToken(Token);
            return client;
        }

        public LanguageClient GetLanguageClient()
        {
            return InternalLogin(new LanguageClient(GetValue("RootAddresses:Contents"), CurrentHttpClient));
        }

        public UserClient GetUserClient()
        {
            return InternalLogin(new UserClient(GetValue("RootAddresses:Authentications"), CurrentHttpClient));
        }

        public RoleClient GetRoleClient()
        {
            return InternalLogin(new RoleClient(GetValue("RootAddresses:Authentications"), CurrentHttpClient));
        }

        public PersonalAccessTokenClient GetPersonalAccessTokenClientClient()
        {
            return InternalLogin(new PersonalAccessTokenClient(GetValue("RootAddresses:Authentications"), CurrentHttpClient));
        }

        public ILoggerProvider GetLogger()
        {
            return ServiceProvider.GetService<ILoggerProvider>();
        }
    }
}
