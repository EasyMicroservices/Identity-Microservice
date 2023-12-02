using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Attributes;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJWTManager _jwtManager;
        private readonly IdentityHelper _identityHelper;
        private readonly IAppUnitOfWork _appUnitOfWork;
        private readonly ClaimManager _claimManager;
        private readonly string _authRoot;

        public AuthenticationController(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
            _identityHelper = _appUnitOfWork.GetIdentityHelper();
            _jwtManager = _appUnitOfWork.GetIJWTManager();
            _claimManager = _appUnitOfWork.GetClaimManager();
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
        [ApplicationInitializeCheck]
        public async Task<MessageContract<RegisterResponseContract>> Register(Contracts.Requests.AddUserRequestContract request)
        {
            return await _identityHelper.Register(request);
        }

        [HttpPost]
        [ApplicationInitializeCheck]
        public async Task<MessageContract<LoginWithTokenResponseContract>> Login(Contracts.Common.UserSummaryContract request)
        {
            var response = await _identityHelper.Login(request);
            if (!response.IsSuccess)
                return response.ToContract<LoginWithTokenResponseContract>();

            UserClient _userClient = _appUnitOfWork.GetUserClient(HttpContext);

            _userClient.SetBearerToken(_appUnitOfWork.GetConfiguration().GetValue<string>("Authorization:FullAccessPAT"));

            var user = await _userClient.GetByIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract { Id = response.Result.UserId }).AsCheckedResult(x => x.Result);
            var roles = await _appUnitOfWork.GetRoleClient(HttpContext).GetRolesByUserIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
            {
                Id = response.Result.UserId
            }).AsCheckedResult(x => x.Result);

            List<ClaimContract> claims = new();

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
                UserId = response.Result.UserId,
                Token = TokenResponse.Result.Token
            };
        }

        [HttpPost]
        [ApplicationInitializeCheck]
        public async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract request)
        {
            var response = await _identityHelper.GenerateToken(request);

            return response;
        }

        [HttpPost]
        [CustomAuthorizeCheck]
        public async Task<MessageContract<UserResponseContract>> RegenerateToken(RegenerateTokenContract request)
        {
            var _userClient = _appUnitOfWork.GetUserClient(HttpContext);

            _userClient.SetBearerToken(_appUnitOfWork.GetConfiguration().GetValue<string>("Authorization:FullAccessPAT"));

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
            var user = await _appUnitOfWork.GetUserClient(HttpContext).GetUserByPersonalAccessTokenAsync(new Authentications.GeneratedServices.PersonalAccessTokenRequestContract()
            {
                Value = request.PersonalAccessToken
            }).AsCheckedResult(x => x.Result);

            var roles = await _appUnitOfWork.GetRoleClient(HttpContext).GetRolesByUserIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
            {
                Id = user.Id
            }).AsCheckedResult(x => x.Result);

            List<ClaimContract> claims = new();

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

            var response = await _jwtManager.GenerateTokenWithClaims(claims);

            SetCookie("token", response.Result.Token);

            return response;
        }
    }
}
