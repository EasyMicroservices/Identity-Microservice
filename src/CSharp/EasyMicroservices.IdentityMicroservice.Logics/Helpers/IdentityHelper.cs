using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class IdentityHelper
    {
        private readonly IConfiguration _config;
        private readonly UserClient _userClient;
        private readonly IJWTManager _jwtManager;
        private readonly string _authRoot;

        public IdentityHelper(IConfiguration config, IJWTManager jwtManager)
        {
            _config = config;
            _jwtManager = jwtManager;
            _authRoot = _config.GetValue<string>("RootAddresses:Authentications");
            _userClient = new(_authRoot, new System.Net.Http.HttpClient());
        }


        public async Task<MessageContract<RegisterResponseContract>> Register(Contracts.Requests.AddUserRequestContract request)
        {
            var usersRecords = await _userClient.GetUserByUserNameAsync(new GetUserByUserNameRequestContract { Username = request.UserName.ToLower() });

            if (usersRecords.IsSuccess)
                return (ServiceContracts.FailedReasonType.Duplicate, "User already exists!");

            var user = await _userClient.AddAsync(new AddUserRequestContract
            {
                UserName = request.UserName,
                Password = request.Password
            });

            return new RegisterResponseContract
            {
                UserId = user.Result,
            };
        }

        public virtual async Task<MessageContract<LoginResponseContract>> Login(Contracts.Common.UserSummaryContract cred)
        {
            var user = await _userClient.VerifyUserIdentityAsync(new Authentications.GeneratedServices.UserSummaryContract { UserName = cred.UserName, Password = cred.Password });
            if (!user.IsSuccess)
                return (ServiceContracts.FailedReasonType.Incorrect, "Username or password is invalid."); //"Username or password is invalid."


            return new LoginResponseContract
            {
                UserId = user.Result.Id
            };
        }

        public virtual async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract userClaim)
        {
            var token = await _jwtManager.GenerateTokenWithClaims(userClaim.Claims);

            return new UserResponseContract
            {
                Token = token.Result.Token
            };
        }
    }
}
