using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Requests
{
    public class GenerateResetPasswordTokenRequestContract
    {
        public string WhiteLabelKey { get; set; }
        public long ExpireTimeInSeconds { get; set; }
        public string UserName { get; set; }
    }
}
