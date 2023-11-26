using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IJWTManager _jwtManager;
        private readonly IdentityHelper _identityHelper;
        private readonly IAppUnitOfWork _appUnitOfWork;
        private readonly string _authRoot;

        public IdentityController(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
            _identityHelper = _appUnitOfWork.GetIdentityHelper();
        }

        [HttpPost]
        public async Task<ServiceContracts.MessageContract> VerifyUserName(VerifyUserRequestContract request)
        {
            var _userClient = _appUnitOfWork.GetUserClient(HttpContext);
            var user = await _userClient.GetByIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract { Id = request.UserId });

            if (!user.IsSuccess)
                return (ServiceContracts.FailedReasonType.NotFound, "User not found");

            if (user.Result.IsUsernameVerified)
                return true;

            await _userClient.UpdateAsync(new Authentications.GeneratedServices.UserContract
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
            var _userClient = _appUnitOfWork.GetUserClient(HttpContext);
            var user = await _userClient.GetByIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
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

        [HttpPost]
        public async Task<MessageContract<UserResponseContract>> LoginByPersonalAccessToken(LoginByPersonalAccessTokenRequestContract request)
        {
            var user = await _appUnitOfWork.GetUserClient(HttpContext).GetUserByPersonalAccessTokenAsync(new Authentications.GeneratedServices.PersonalAccessTokenRequestContract()
            {
                Value = request.PersonalAccessToken
            }).AsCheckedResult(x => x.Result);

            var roles = await _appUnitOfWork.GetRoleClient(HttpContext).GetRolesByUserIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
            {
                Id = user.Id
            }).AsCheckedResult(x => x.Result);

            var response = await _identityHelper.GenerateToken(new UserClaimContract()
            {
                UserName = user.UserName,
                Password = user.Password,
                Claims = roles.Select(x => new ClaimContract()
                {
                    Name = ClaimTypes.Role,
                    Value = x.Name
                }).ToList()
            });

            return response;
        }
    }
}
