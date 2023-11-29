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

namespace EasyMicroservices.IdentityMicroservice.Interfaces
{
    public interface IAppUnitOfWork : IUnitOfWork
    {
        public IJWTManager GetIJWTManager();
        public IConfiguration GetConfiguration();
        public IdentityHelper GetIdentityHelper();
        public LanguageClient GetLanguageClient();
        public ClaimManager GetClaimManager();
        public UserClient GetUserClient(HttpContext httpContext);
        public RoleClient GetRoleClient(HttpContext httpContext);
    }
}
