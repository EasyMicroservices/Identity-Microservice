using Authentications.GeneratedServices;
using EasyMicroservices.Cores.AspCoreApi;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi.Interfaces;
using EasyMicroservices.Cores.Database.Interfaces;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UsersClient _userClient;
        private readonly IJWTManager _jwtManager;
        private readonly IdentityHelper _identityHelper;
        private readonly IAppUnitOfWork _appUnitOfWork;
        private readonly string _authRoot;

        public IdentityController(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
            _config = _appUnitOfWork.GetConfiguration();
            _authRoot = _config.GetValue<string>("RootAddresses:Authentications");
            _userClient = new(_authRoot, new System.Net.Http.HttpClient());
            _identityHelper = _appUnitOfWork.GetIdentityHelper();
        }

        [HttpPost]
        public async Task<ServiceContracts.MessageContract> VerifyUserName(VerifyUserRequestContract request)
        {
            var user = await _userClient.GetByIdAsync(new Int64GetIdRequestContract { Id = request.UserId });

            if (!user.IsSuccess)
                return (ServiceContracts.FailedReasonType.NotFound, "User not found");

            if (user.Result.IsUsernameVerified)
                return true;

            await _userClient.UpdateAsync(new UserContract
            {
                CreationDateTime = user.Result.CreationDateTime,
                DeletedDateTime = user.Result.DeletedDateTime,
                Id = user.Result.Id,
                IsDeleted = user.Result.IsDeleted,
                IsUsernameVerified = true,
                ModificationDateTime = user.Result.ModificationDateTime,
                Password = user.Result.Password,
                UniqueIdentity = user.Result.UniqueIdentity,
                UserName = user.Result.UserName,
            }).AsCheckedResult();

            return true;
        }

        [HttpPost]
        public async Task<MessageContract<RegisterResponseContract>> Register(Contracts.Requests.AddUserRequestContract request)
        {
            return await _identityHelper.Register(request);
        }

        [HttpPost]
        public async Task<MessageContract<LoginResponseContract>> Login(Contracts.Common.UserSummaryContract request)
        {
            request.Password = await SecurityHelper.HashPassword(request.Password);
            var response = await _identityHelper.Login(request);
            return response;
        }

        [HttpPost]
        public async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract request)
        {
            string password = await SecurityHelper.HashPassword(request.Password);
            request.Password = password;

            var response = await _identityHelper.GenerateToken(request);

            return response;
        }

        [HttpPost]
        public async Task<MessageContract<UserResponseContract>> RegenerateToken(RegenerateTokenContract request)
        {
            var user = await _userClient.GetByIdAsync(new Int64GetIdRequestContract
            {
                Id = request.UserId
            });

            if (user.IsSuccess)
            {

                string password = user.Result.Password;

                var req = new UserClaimContract
                {
                    Password = password,
                    UserName = user.Result.UserName,
                    Claims = request.Claims
                };

                var response = await _identityHelper.GenerateToken(req);

                return new UserResponseContract
                {
                    Token = response.Result.Token
                };
            }

            return user.ToContract<UserResponseContract>();
        }
    }
}
