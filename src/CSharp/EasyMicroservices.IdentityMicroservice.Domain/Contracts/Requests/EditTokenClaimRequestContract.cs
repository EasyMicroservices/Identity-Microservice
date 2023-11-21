using Authentications.GeneratedServices;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Requests
{
    public class EditTokenClaimRequestContract
    {
        public string Token { get; set; }
        public List<ClaimContract> Claims { get; set; }
    }
}
