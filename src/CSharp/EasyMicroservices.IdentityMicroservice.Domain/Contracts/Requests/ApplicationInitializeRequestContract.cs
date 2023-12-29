using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Common
{
    public class ApplicationInitializeRequestContract
    {
        public string WhiteLabelKey { get; set; }
        public string Language { get; set; }
    }
}
