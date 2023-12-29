using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class IdentityHelper
    {
        IAppUnitOfWork _appUnitOfWork;

        public IdentityHelper(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }


        public async Task<MessageContract<RegisterResponseContract>> Register(Contracts.Requests.AddUserRequestContract request)
        {
            var whiteLabelClient = _appUnitOfWork.GetWhiteLabelClient();
            var uniqueIdentityOfBusiness = await whiteLabelClient.GetUniqueIdentityByKeyAsync(new WhiteLables.GeneratedServices.GuidGetByIdRequestContract()
            {
                Id = System.Guid.Parse(request.WhiteLabelKey)
            }).AsCheckedResult(x => x.Result);

            var client = _appUnitOfWork.GetUserClient();
            var user = await client.GetUserByUserNameAsync(new Authentications.GeneratedServices.GetUserByUserNameRequestContract
            {
                UserName = request.UserName.ToLower(),
                UniqueIdentity = uniqueIdentityOfBusiness
            });
            if (user.IsSuccess)
                return (FailedReasonType.Duplicate, $"User with UserName: {request.UserName} already exists!");
            var addedUserId = await client.AddAsync(new Authentications.GeneratedServices.AddUserRequestContract
            {
                UserName = request.UserName,
                Password = request.Password,
                UniqueIdentity = uniqueIdentityOfBusiness
            }).AsCheckedResult(x => x.Result);

            return new RegisterResponseContract
            {
                UserId = addedUserId,
            };
        }

        public virtual async Task<LoginResponseContract> Login(Contracts.Common.UserSummaryContract request)
        {
            var _whiteLabelClient = _appUnitOfWork.GetWhiteLabelClient();
            var uniqueIdentity = await _whiteLabelClient.GetUniqueIdentityByKeyAsync(new WhiteLables.GeneratedServices.GuidGetByIdRequestContract
            {
                Id = Guid.Parse(request.WhiteLabelKey)
            }).AsCheckedResult(x => x.Result);
            var client = _appUnitOfWork.GetUserClient();
            var user = await client.VerifyUserIdentityAsync(new Authentications.GeneratedServices.UserSummaryContract
            {
                UserName = request.UserName,
                Password = request.Password,
                UniqueIdentity = uniqueIdentity
            }).AsCheckedResult(x => x.Result);


            return new LoginResponseContract
            {
                UserId = user.Id
            };
        }

        public Task<string> GetFullAccessPersonalAccessToken()
        {
            var ownerPat = _appUnitOfWork.GetConfiguration().GetValue<string>("Authorization:FullAccessPAT");
            return GetFullAccessPersonalAccessToken(ownerPat);
        }

        public async Task<string> GetFullAccessPersonalAccessToken(string personalAccessToken)
        {
            var user = await _appUnitOfWork.GetUserClient().GetUserByPersonalAccessTokenAsync(new Authentications.GeneratedServices.PersonalAccessTokenRequestContract()
            {
                Value = personalAccessToken
            }).AsCheckedResult(x => x.Result);

            var roles = await _appUnitOfWork.GetRoleClient().GetRolesByUserIdAsync(new Authentications.GeneratedServices.GetByIdAndUniqueIdentityRequestContract
            {
                Id = user.Id,
                //UniqueIdentity = user.UniqueIdentity
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

            var response = await _appUnitOfWork.GetIJWTManager()
                .GenerateTokenWithClaims(claims)
                .AsCheckedResult();
            return response.Token;
        }

        public virtual async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract userClaim)
        {
            await Login(userClaim);

            var token = await _appUnitOfWork.GetIJWTManager().GenerateTokenWithClaims(userClaim.Claims);

            return new UserResponseContract
            {
                Token = token.Result.Token
            };
        }
    }
}
