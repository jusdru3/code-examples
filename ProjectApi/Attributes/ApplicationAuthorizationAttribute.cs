using ProjectApi.Core.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectApi.Attributes;

public class ApplicationAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    private readonly bool _allowUninitialized;

    public ApplicationAuthorizationAttribute(bool allowUninitialized = false)
    {
        _allowUninitialized = allowUninitialized;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var claims = context.HttpContext.User.Claims;
        var applicationUserIdClaim = claims.FirstOrDefault(c => c.Type == AuthenticationClaims.ApplicationUserId);
        var hasApplicationUserId = applicationUserIdClaim != null && Guid.TryParse(applicationUserIdClaim.Value, out _);

        var accessClaimValue = claims.FirstOrDefault(c => c.Type == AuthenticationClaims.Access)?.Value ?? "false";
        var access = bool.Parse(accessClaimValue);

        if (!hasApplicationUserId || !access)
        {
            context.Result = new UnauthorizedObjectResult(string.Empty);
        }

        if (!_allowUninitialized)
        {
            var initializedClaimValue =
                claims.FirstOrDefault(c => c.Type == AuthenticationClaims.Initialized)?.Value ?? "false";
            var initialized = bool.Parse(initializedClaimValue);

            if (!initialized)
            {
                context.Result = new UnauthorizedObjectResult(string.Empty);
            }
        }
    }
}