using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Attributes;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAppUnitOfWork _appUnitOfWork;

        public AuthenticationController(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }

        private void SetCookie(string key, string value)
        {
            bool hasSSL = _appUnitOfWork.GetConfiguration().GetValue<bool>("HasSSL");
            if (hasSSL)
            {
                var cookieOptions = new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    HttpOnly = true,
                    Secure = true
                };
                Response.Cookies.Append(key, value, cookieOptions);
            }
            else
                Response.Cookies.Append(key, value);
        }

        [HttpPost]
        public async Task<ServiceContracts.MessageContract> VerifyUserName(VerifyUserRequestContract request)
        {
            var _userClient = _appUnitOfWork.GetUserClient();
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
            var _identityHelper = _appUnitOfWork.GetIdentityHelper();
            return await _identityHelper.Register(request);
        }

        [HttpPost]
        public async Task<MessageContract<LoginWithTokenResponseContract>> Login(Contracts.Common.UserSummaryContract request)
        {
            var _identityHelper = _appUnitOfWork.GetIdentityHelper();
            var response = await _identityHelper.Login(request);

            var user = await _appUnitOfWork.GetUserClient()
                .GetByIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract { Id = response.UserId })
                .AsCheckedResult(x => x.Result);

            var roles = await _appUnitOfWork.GetRoleClient()
                .GetRolesByUserIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
                {
                    Id = response.UserId
                }).AsCheckedResult(x => x.Result);

            List<ClaimContract> claims = new();

            var _claimManager = _appUnitOfWork.GetClaimManager();
            _claimManager.SetCurrentLanguage(_claimManager.CurrentLanguage, claims);
            if (!_claimManager.HasId())
            {
                _claimManager.SetId(user.Id, claims);
                _claimManager.SetUniqueIdentity(user.UniqueIdentity, claims);
                _claimManager.SetRole(roles.Select(x => new ClaimContract()
                {
                    Name = ClaimTypes.Role,
                    Value = x.Name
                }).ToList(), claims);
            }

            var TokenResponse = await _appUnitOfWork.GetIJWTManager().GenerateTokenWithClaims(claims);

            SetCookie("token", TokenResponse.Result.Token);

            return new LoginWithTokenResponseContract
            {
                UserId = response.UserId,
                Token = TokenResponse.Result.Token
            };
        }

        [HttpPost]
        [ApplicationInitializeCheck]
        public async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract request)
        {
            request.Claims = request.Claims.Where(x => !x.Name.Contains("Role", StringComparison.OrdinalIgnoreCase)).ToList();
            var _identityHelper = _appUnitOfWork.GetIdentityHelper();
            var response = await _identityHelper.GenerateToken(request);

            return response;
        }

        [HttpPost]
        [CustomAuthorizeCheck]
        public async Task<MessageContract<UserResponseContract>> RegenerateToken(RegenerateTokenContract request)
        {
            request.Claims = request.Claims.Where(x => !x.Name.Contains("Role", StringComparison.OrdinalIgnoreCase)).ToList();
            var _userClient = _appUnitOfWork.GetUserClient();

            _userClient.SetBearerToken(_appUnitOfWork.GetConfiguration().GetValue<string>("Authorization:FullAccessPAT"));

            var _claimManager = _appUnitOfWork.GetClaimManager();
            var user = await _userClient.GetByIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
            {
                Id = _claimManager.Id
            });

            if (!user.IsSuccess)
                return user.ToContract<UserResponseContract>();


            string password = user.Result.Password;

            var req = new UserClaimContract
            {
                Password = password,
                UserName = user.Result.UserName,
                Claims = request.Claims
            };

            var _identityHelper = _appUnitOfWork.GetIdentityHelper();
            var response = await _identityHelper.GenerateToken(req);

            return new UserResponseContract
            {
                Token = response.Result.Token
            };


        }

        [HttpPost]
        public async Task<MessageContract<ApplicationInitializeResponseContract>> ApplicationInitialize(ApplicationInitializeRequestContract request)
        {
            LanguageClient _languageClient = _appUnitOfWork.GetLanguageClient();

            var language = await _languageClient.HasLanguageAsync(new HasLanguageRequestContract
            {
                Language = request.Language
            });

            if (!language.IsSuccess)
                return (ServiceContracts.FailedReasonType.NotFound, "Language not found.");

            List<ClaimContract> claims = new();
            var _claimManager = _appUnitOfWork.GetClaimManager();
            _claimManager.SetCurrentLanguage(request.Language, claims);
            if (_claimManager.HasId())
            {
                _claimManager.SetId(_claimManager.Id, claims);
                _claimManager.SetRole(_claimManager.Role.Select(o => new ClaimContract() { Name = ClaimTypes.Name, Value = o }).ToList(), claims);
            }

            var TokenResponse = await _appUnitOfWork.GetIJWTManager().GenerateTokenWithClaims(claims);

            SetCookie("token", TokenResponse.Result.Token);

            return new ApplicationInitializeResponseContract() { IsLogin = _claimManager.HasId(), Token = TokenResponse.Result.Token };
        }

        [HttpPost]
        public async Task<MessageContract<UserResponseContract>> LoginByPersonalAccessToken(LoginByPersonalAccessTokenRequestContract request)
        {
            var token = await _appUnitOfWork.GetIdentityHelper().GetFullAccessPersonalAccessToken(request.PersonalAccessToken);
            SetCookie("token", token);

            return new UserResponseContract()
            {
                Token = token,
            };
        }
    }
}
