
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using System.Text;
using EasyMicroservices.ServiceContracts;
using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Requests;
using System.Collections.Generic;

namespace EasyMicroservices.IdentityMicroservice.Interfaces
{
    public interface IJWTManager
    {
        Task<ListMessageContract<ClaimContract>> GetClaimsFromToken(string token);      
        Task<MessageContract<UserResponseContract>> EditTokenClaims(EditTokenClaimRequestContract request);
        Task<MessageContract<UserResponseContract>> GenerateTokenWithClaims(List<ClaimContract> claims);
    }
}