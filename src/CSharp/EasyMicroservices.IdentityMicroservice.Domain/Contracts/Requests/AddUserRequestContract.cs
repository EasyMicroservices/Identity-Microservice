using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Requests
{
    public class AddUserRequestContract
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
