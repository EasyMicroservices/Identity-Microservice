using EasyMicroservices.IdentityMicroservice;
using EasyMicroservices.Cores.AspCoreApi;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi.Interfaces;
using EasyMicroservices.Cores.Contracts.Requests;
using EasyMicroservices.Cores.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using EasyMicroservices.IdentityMicroservice.Helpers;
using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.Logger.Interfaces;
using WhiteLables.GeneratedServices;

namespace EasyMicroservices.IdentityMicroservice.Interfaces
{
    public interface IAppUnitOfWork : IUnitOfWork
    {
        public IHttpContextAccessor GetHttpContextAccessor();
        public IJWTManager GetIJWTManager();
        public ILoggerProvider GetLogger();
        public IdentityHelper GetIdentityHelper();
        public ClaimManager GetClaimManager();
        public LanguageClient GetLanguageClient();
        public UserClient GetUserClient();
        public RoleClient GetRoleClient();
        public PersonalAccessTokenClient GetPersonalAccessTokenClientClient();
        public WhiteLabelClient GetWhiteLabelClient();
    }
}
