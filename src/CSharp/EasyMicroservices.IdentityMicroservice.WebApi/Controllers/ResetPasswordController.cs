using Authentications.GeneratedServices;
using Contents.GeneratedServices;
using EasyMicroservices.Cores.AspCoreApi;
using EasyMicroservices.Cores.Database.Managers;
using EasyMicroservices.IdentityMicroservice.Attributes;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text;
using MessageContract = EasyMicroservices.ServiceContracts.MessageContract;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IAppUnitOfWork _appUnitOfWork;

        public ResetPasswordController(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract<GenerateResetPasswordTokenResponseContract>> GenerateResetPasswordToken(GenerateResetPasswordTokenRequestContract request)
        {
            var client = _appUnitOfWork.GetResetPasswordTokenClientClient();
            var _whiteLabelClient = _appUnitOfWork.GetWhiteLabelClient();

            var uniqueIdentity = await _whiteLabelClient.GetUniqueIdentityByKeyAsync(new WhiteLables.GeneratedServices.GuidGetByIdRequestContract
            {
                Id = Guid.Parse(request.WhiteLabelKey)
            }).AsCheckedResult(x => x.Result);

            var user = await _appUnitOfWork.GetUserClient().GetUserByUserNameAsync(new Authentications.GeneratedServices.GetUserByUserNameRequestContract
            {
                UserName = request.UserName,
                UniqueIdentity = uniqueIdentity
            }).AsCheckedResult(x => x.Result);

            var previousToken = await client.GetAllByUniqueIdentityAsync(new Authentications.GeneratedServices.GetByUniqueIdentityRequestContract { UniqueIdentity = user.UniqueIdentity }); ;

            if (previousToken.IsSuccess)
                await client.UpdateBulkChangedValuesOnlyAsync(new ResetPasswordTokenContractUpdateBulkRequestContract
                {
                    Items = previousToken.Result.Select(x => new ResetPasswordTokenContract { Id = x.Id, HasConsumed = true, Token = x.Token, ExpirationDateTime = x.ExpirationDateTime, UniqueIdentity = x.UniqueIdentity }).ToList()
                });

            string token = string.Join("", _appUnitOfWork.GetSHA256HashProvider().Compute(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).Select(b => b.ToString("X2")));

            var addTokenResponse = await client.AddAsync(new ResetPasswordTokenContract
            {
                UniqueIdentity = user.UniqueIdentity,
                Token = token,
                ExpirationDateTime = DateTime.Now.AddSeconds(request.ExpireTimeInSeconds),
            });

            if (!addTokenResponse.IsSuccess)
                return addTokenResponse.ToContract<GenerateResetPasswordTokenResponseContract>();

            return new GenerateResetPasswordTokenResponseContract { Token = token };
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract> ValidateResetPasswordToken(ValidateResetPasswordTokenRequestContract request)
        {
            var client = _appUnitOfWork.GetResetPasswordTokenClientClient();
            var validateResponse = await client.GetValidTokenAsync(new GetValidTokenRequestContract { Token = request.Token });
            return _appUnitOfWork.GetMapper().Map<MessageContract>(validateResponse);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract> ConsumeResetPasswordToken(ConsumeResetPasswordTokenRequestContract request)
        {
            var client = _appUnitOfWork.GetResetPasswordTokenClientClient();

            var token = await client.GetValidTokenAsync(new GetValidTokenRequestContract { Token = request.Token }).AsCheckedResult(x => x.Result);

            var user = await _appUnitOfWork.GetUserClient().GetByUniqueIdentityAsync(new Authentications.GeneratedServices.GetByUniqueIdentityRequestContract { UniqueIdentity = DefaultUniqueIdentityManager.CutUniqueIdentity(token.UniqueIdentity, 4), Type = Authentications.GeneratedServices.GetUniqueIdentityType.Equals }).AsCheckedResult(x => x.Result);
            var updateUserResponse = await _appUnitOfWork.GetUserClient().UpdateChangedValuesOnlyAsync(new Authentications.GeneratedServices.UserContract
            {
                Id = user.Id,
                Password = request.Password
            }).AsCheckedResult(x => x.Result);

            token.HasConsumed = true;

            var updateTokenResponse = await client.UpdateChangedValuesOnlyAsync(token).AsCheckedResult(x => x.Result);

            return true;
        }
    }
}
