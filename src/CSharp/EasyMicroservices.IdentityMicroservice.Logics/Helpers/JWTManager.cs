using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FailedReasonType = EasyMicroservices.ServiceContracts.FailedReasonType;

namespace EasyMicroservices.IdentityMicroservice.Services
{
    public class JWTManager : IJWTManager
    {
        private readonly IConfiguration _config;
        private readonly UserClient _userClient;
        private readonly string _authRoot;

        public JWTManager(IConfiguration config)
        {
            _config = config;
            _authRoot = _config.GetValue<string>("RootAddresses:Authentications");
            _userClient = new(_authRoot, new System.Net.Http.HttpClient());
        }

        public async Task<MessageContract<UserResponseContract>> GenerateTokenWithClaims(List<ClaimContract> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.GetValue<string>("Authorization:JWT:Key"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Select(x => new Claim(x.Name, x.Value)).ToArray()),
                Expires = DateTime.UtcNow.AddSeconds(_config.GetValue<int>("Authorization:JWT:TokenExpireTimeInSeconds")),
                Issuer = _config.GetValue<string>("Authorization:JWT:Issuer"),
                Audience = _config.GetValue<string>("Authorization:JWT:Audience"),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new UserResponseContract
            {
                Token = tokenString,
            };
        }

        public async Task<MessageContract<UserResponseContract>> EditTokenClaims(EditTokenClaimRequestContract request)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = (JwtSecurityToken)tokenHandler.ReadToken(request.Token);

            List<ClaimContract> newClaims = new();

            foreach (Claim claim in securityToken.Claims)
            {
                if (request.Claims.Any(o => o.Name == claim.Type))
                    newClaims.Add(new ClaimContract
                    {
                        Name = claim.Type,
                        Value = request.Claims.Where(o => o.Name == claim.Type).FirstOrDefault().Value
                    });
                else
                    newClaims.Add(new ClaimContract
                    {
                        Name = claim.Type,
                        Value = claim.Value
                    });

            }

            var token = await GenerateTokenWithClaims(newClaims);

            if (token)
                return token;


            return (FailedReasonType.Unknown, "An error has occured");
        }


        public async Task<ListMessageContract<ClaimContract>> GetClaimsFromToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            List<ClaimContract> newClaims = securityToken.Claims.Select(o => new ClaimContract
            {
                Name = o.Type,
                Value = o.Value
            }).ToList();

            return newClaims;
        }

        //public virtual async Task<MessageContract<long>> Login(UserSummaryContract cred)
        //{
        //    var logic = _unitOfWork.GetLongContractLogic<UserEntity, AddUserRequestContract, UserContract, UserContract>();
        //    var user = await logic.GetBy(x => x.UserName == cred.UserName && x.Password == cred.Password);
        //    if (!user.IsSuccess)
        //        return (FailedReasonType.AccessDenied, "Username or password is invalid."); //"Username or password is invalid."


        //    return user.Result.Id;
        //}

        //public virtual async Task<MessageContract<UserResponseContract>> GenerateToken(UserClaimContract cred)
        //{
        //    var response = await Login(cred);
        //    if (!response)
        //        return response.ToContract<UserResponseContract>();

        //    var logic = _unitOfWork.GetLongContractLogic<UserEntity, AddUserRequestContract, UserContract, UserContract>();
        //    var user = await logic.GetBy(x => x.UserName == cred.UserName && x.Password == cred.Password);

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.UTF8.GetBytes(_config.GetValue<string>("JWT:Key"));
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(cred.Claims.Select(x => new Claim(x.Name, x.Value)).ToArray()),
        //        Expires = DateTime.UtcNow.AddSeconds(_config.GetValue<int>("JWT:TokenExpireTimeInSeconds")),
        //        Issuer = _config.GetValue<string>("JWT:Issuer"),
        //        Audience = _config.GetValue<string>("JWT:Audience"),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    var tokenString = tokenHandler.WriteToken(token);

        //    return new UserResponseContract
        //    {
        //        Token = tokenString,
        //        UniqueIdentity = user.Result.UniqueIdentity
        //    };
        //}
    }
}
