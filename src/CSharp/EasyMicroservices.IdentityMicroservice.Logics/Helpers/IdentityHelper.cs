//using Authentications.GeneratedServices;
using Authentications.GeneratedServices;
using EasyMicroservices.AuthenticationsMicroservice.Clients;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        //public virtual async Task<MessageContract<long>> Register(AddUserRequestContract input)
        //{
        //    string Password = input.Password;
        //    input.Password = await SecurityHelper.HashPassword(input.Password);

        //    //var logic = _unitOfWork.GetLongContractLogic<UserEntity, AddUserRequestContract, UserContract, UserContract>();
        //    //var usersRecords = await logic.GetBy(x => x.UserName == input.UserName.ToLower());

        //    //if (usersRecords.IsSuccess)
        //    //    return (FailedReasonType.Duplicate, "User already exists!");

        //    //var user = await logic.Add(input);

        //    return user;
        //}
    }
}
