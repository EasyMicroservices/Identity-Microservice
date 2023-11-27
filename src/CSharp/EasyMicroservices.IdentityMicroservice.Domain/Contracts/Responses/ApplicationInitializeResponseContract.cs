using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Responses;

{
    public class ApplicationInitializeResponseContract
    {
        public bool IsLogin { get; set; }
        //public bool IsAdmin { get; set; }
    }
}
