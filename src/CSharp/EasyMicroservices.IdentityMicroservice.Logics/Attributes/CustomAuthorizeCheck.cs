using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.IdentityMicroservice.Attributes
{
    public class CustomAuthorizeCheckAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public string[] ClaimTypes { get; set; }
        public CustomAuthorizeCheckAttribute(params string[] claimTypes)
        {
            ClaimTypes = claimTypes.Length > 0 ? claimTypes : new[] { "Id" };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasClaims = ClaimTypes.All(o => user.Claims.Any(x => x.Type == o));

            if (!hasClaims)
                context.Result = new UnauthorizedResult();

            return;
        }
    }
}
