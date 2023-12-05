using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Text.Json;

namespace EasyMicroservices.IdentityMicroservice.Attributes
{
    public class ApplicationInitializeCheck : AuthorizeAttribute, IAuthorizationFilter
    {
        public string[] ClaimTypes { get; set; }
        public ApplicationInitializeCheck(params string[] claimTypes)
        {
            ClaimTypes = claimTypes.Length > 0 ? claimTypes : new[] { "CurrentLanguage" };
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = GetErrorContent();
                return;
            }

            var hasClaims = ClaimTypes.All(o => user.Claims.Any(x => x.Type == o));

            if (!hasClaims)
                context.Result = GetErrorContent();

            return;
        }

        ContentResult GetErrorContent()
        {
            var msg = (MessageContract)(FailedReasonType.SessionAccessDenied, "Please call appinit!");
            return new ContentResult()
            {
                Content = JsonSerializer.Serialize(msg),
                ContentType = "application/json"
            };
        }
    }
}
