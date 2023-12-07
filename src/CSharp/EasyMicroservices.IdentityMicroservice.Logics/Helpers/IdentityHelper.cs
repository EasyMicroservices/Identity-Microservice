using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FailedReasonType = EasyMicroservices.ServiceContracts.FailedReasonType;

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
            var client = _appUnitOfWork.GetUserClient();
            var usersRecords = await client.GetUserByUserNameAsync(new GetUserByUserNameRequestContract
            {
                Username = request.UserName.ToLower()
            }).AsCheckedResult(x => x.Result);

            var user = await client.AddAsync(new AddUserRequestContract
            {
                UserName = request.UserName,
                Password = request.Password
            });

            return new RegisterResponseContract
            {
                UserId = user.Result,
            };
        }

        public virtual async Task<LoginResponseContract> Login(Contracts.Common.UserSummaryContract cred)
        {
            var client = _appUnitOfWork.GetUserClient();
            var user = await client.VerifyUserIdentityAsync(new Authentications.GeneratedServices.UserSummaryContract
            {
                UserName = cred.UserName,
                Password = cred.Password
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

            var roles = await _appUnitOfWork.GetRoleClient().GetRolesByUserIdAsync(new Authentications.GeneratedServices.Int64GetIdRequestContract
            {
                Id = user.Id
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
