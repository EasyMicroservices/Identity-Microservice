using Contents.GeneratedServices;
using EasyMicroservices.Cores.AspCoreApi;
using EasyMicroservices.Cores.Database.Managers;
using EasyMicroservices.IdentityMicroservice.Attributes;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using EasyMicroservices.IdentityMicroservice.Contracts.Responses;
using EasyMicroservices.IdentityMicroservice.Database.Entities;
using EasyMicroservices.IdentityMicroservice.Helpers;
using EasyMicroservices.IdentityMicroservice.Interfaces;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
using MessageContract = EasyMicroservices.ServiceContracts.MessageContract;

namespace EasyMicroservices.IdentityMicroservice.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ResetPasswordController : SimpleQueryServiceController<ResetPasswordTokenEntity, ResetPasswordTokenContract, ResetPasswordTokenContract, ResetPasswordTokenContract, long>
    {
        private readonly IAppUnitOfWork _appUnitOfWork;

        public ResetPasswordController(IAppUnitOfWork appUnitOfWork) : base(appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract> GenerateResetPasswordToken(GenerateResetPasswordTokenRequestContract request)
        {
            var logic = _appUnitOfWork.GetContractLogic<ResetPasswordTokenEntity, ResetPasswordTokenContract, long>();

            var user = await _appUnitOfWork.GetUserClient().GetByUniqueIdentityAsync(new Authentications.GeneratedServices.GetByUniqueIdentityRequestContract { UniqueIdentity = request.UniqueIdentity, Type = Authentications.GeneratedServices.GetUniqueIdentityType.All }).AsCheckedResult(x => x.Result);

            var previousToken = await logic.GetAll(x => x.Where(o => o.UniqueIdentity.StartsWith(user.UniqueIdentity)));

            if (!previousToken.IsSuccess)
                await logic.UpdateBulkChangedValuesOnly(previousToken.Result.Select(x => new ResetPasswordTokenContract { Id = x.Id, HasConsumed = true}).ToList());

            string token = SecurityHelper.Hash(Guid.NewGuid().ToString());

            return await logic.Add(new ResetPasswordTokenContract
            {
                UniqueIdentity = user.UniqueIdentity,
                Token = token,
                ExpirationDateTime = DateTime.Now.AddSeconds(request.ExpireTimeInSeconds),
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract> ValidateResetPasswordToken(ValidateResetPasswordTokenRequestContract request)
        {
            var logic = _appUnitOfWork.GetContractLogic<ResetPasswordTokenEntity, ResetPasswordTokenContract, long>();
            return await logic.GetBy(x => x.Token.Equals(request.Token) && !x.HasConsumed && !x.IsDeleted && x.ExpirationDateTime >= DateTime.Now);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageContract> ConsumeResetPasswordToken(ConsumeResetPasswordTokenRequestContract request)
        {
            var logic = _appUnitOfWork.GetContractLogic<ResetPasswordTokenEntity, ResetPasswordTokenContract, long>();

            var token = await logic.GetBy(x => x.Token.Equals(request.Token) && !x.HasConsumed && !x.IsDeleted && x.ExpirationDateTime >= DateTime.Now).AsCheckedResult(x => x.Result);
            var user = await _appUnitOfWork.GetUserClient().GetByUniqueIdentityAsync(new Authentications.GeneratedServices.GetByUniqueIdentityRequestContract { UniqueIdentity = DefaultUniqueIdentityManager.CutUniqueIdentity(token.UniqueIdentity, 4), Type = Authentications.GeneratedServices.GetUniqueIdentityType.All }).AsCheckedResult(x => x.Result);
            var updateUserResponse = await _appUnitOfWork.GetUserClient().UpdateChangedValuesOnlyAsync(new Authentications.GeneratedServices.UserContract
            {
                Id = user.Id,
                Password = request.Password
            }).AsCheckedResult(x => x.Result);

            token.HasConsumed = true;

            return await logic.UpdateChangedValuesOnly(token);
        }
    }
}
