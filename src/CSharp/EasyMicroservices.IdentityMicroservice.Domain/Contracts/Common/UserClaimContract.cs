using System.Collections.Generic;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Common
{
    public class UserClaimContract : UserSummaryContract
    {
        public List<ClaimContract> Claims { get; set; }
    }
}
