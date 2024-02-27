
using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.Clients;
using EasyMicroservices.Cores.Models;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.Logger.Interfaces;
using EasyMicroservices.Security;
using EasyMicroservices.Security.Interfaces;
using EasyMicroservices.Security.Providers.HashProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using WhiteLables.GeneratedServices;

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

        public SHA256HashProvider GetSHA256HashProvider()
        {
            return _service.GetService<SHA256HashProvider>();
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
        static List<ServiceAddressInfo> ServiceAddresses;
        string GetValue(string key)
        {
            if (ServiceAddresses == null)
                ServiceAddresses = GetConfiguration().GetSection("ServiceAddresses").Get<List<ServiceAddressInfo>>();
            return ServiceAddresses.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase)).Address;
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
            return InternalLogin(new LanguageClient(GetValue("Content"), CurrentHttpClient));
        }

        public UserClient GetUserClient()
        {
            return InternalLogin(new UserClient(GetValue("Authentication"), CurrentHttpClient));
        }

        public RoleClient GetRoleClient()
        {
            return InternalLogin(new RoleClient(GetValue("Authentication"), CurrentHttpClient));
        }

        public ResetPasswordTokenClient GetResetPasswordTokenClientClient()
        {
            return InternalLogin(new ResetPasswordTokenClient(GetValue("Authentication"), CurrentHttpClient));
        }

        public PersonalAccessTokenClient GetPersonalAccessTokenClientClient()
        {
            return InternalLogin(new PersonalAccessTokenClient(GetValue("Authentication"), CurrentHttpClient));
        }

        public ILoggerProvider GetLogger()
        {
            return ServiceProvider.GetService<ILoggerProvider>();
        }

        public WhiteLabelClient GetWhiteLabelClient()
        {
            return InternalLogin(new WhiteLabelClient(GetValue("WhiteLabel"), CurrentHttpClient));
        }
    }
}
