using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class IdentityHelper
    {
        private readonly IConfiguration _config;
        private readonly UsersClient _userClient;
        private readonly IJWTManager _jwtManager;
        private readonly string _authRoot;

        public IdentityHelper(IConfiguration config, IJWTManager jwtManager)
        {
            _config = config;
            _jwtManager = jwtManager;
            _authRoot = _config.GetValue<string>("RootAddresses:Authentications");
            _userClient = new(_authRoot, new System.Net.Http.HttpClient());
        }


        public async Task<MessageContract<long>> Register(Contracts.Requests.AddUserRequestContract request)
        {
            request.Password = await SecurityHelper.HashPassword(request.Password);

            var usersRecords = await _userClient.UserHasExistsByUsernameAsync(new UserHasExistsByUsernameRequestContract { Username = request.UserName.ToLower()});

            if (usersRecords.IsSuccess)
                return (ServiceContracts.FailedReasonType.Duplicate, "User already exists!");

            var user = await _userClient.AddAsync(new AddUserRequestContract
            {
                UserName = request.UserName,
                Password = request.Password
            });

            return user.Result;
        }

        public virtual async Task<MessageContract<long>> Login(Contracts.Common.UserSummaryContract cred)
        {
            var user = await _userClient.VerifyUserIdentityAsync(new Authentications.GeneratedServices.UserSummaryContract { UserName = cred.UserName, Password = cred.Password});
            if (!user.IsSuccess)
                return (ServiceContracts.FailedReasonType.AccessDenied, "Username or password is invalid."); //"Username or password is invalid."

            
            return user.Result.Id;
        }

        public virtual async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract cred)
        {
            var response = await Login(cred);
            if (!response)
                return response.ToContract<UserResponseContract>();

            var user = await _userClient.VerifyUserIdentityAsync(new Authentications.GeneratedServices.UserSummaryContract { UserName = cred.UserName, Password = cred.Password });

            var token = await _jwtManager.GenerateTokenWithClaims(cred.Claims);

            return new UserResponseContract
            {
                Token = token.Result.Token,
                UniqueIdentity = user.Result.UniqueIdentity
            };
        }

    }
}
