using ProjectApi.Core.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectApi.Attributes;

public class BackOfficeAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var claims = context.HttpContext.User.Claims;
        var backOfficeUserIdClaim = claims.FirstOrDefault(c => c.Type == AuthenticationClaims.BackOfficeUserId);
        var hasBackOfficeUserId = backOfficeUserIdClaim != null && Guid.TryParse(backOfficeUserIdClaim.Value, out _);

        var accessClaimValue = claims.FirstOrDefault(c => c.Type == AuthenticationClaims.Access)?.Value ?? "false";
        var access = bool.Parse(accessClaimValue);

        if (hasBackOfficeUserId && access) return;

        context.Result = new UnauthorizedObjectResult(string.Empty);
    }
}