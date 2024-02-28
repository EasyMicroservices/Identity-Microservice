using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Responses
{
    public class ValidateResetPasswordTokenResponseContract
    {
        public string UserName { get; set; }
    }
}
