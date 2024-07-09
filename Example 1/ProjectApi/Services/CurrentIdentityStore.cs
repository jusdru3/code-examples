using ProjectApi.Core.Authentication;
using ProjectApi.Core.Authorization.Dtos;
using ProjectApi.Core.Authorization.Services.Interfaces;
using ProjectApi.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Services;

public class CurrentIdentityStore : ICurrentIdentityStore
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private CurrentIdentityData? _currentIdentityData;

    public CurrentIdentityStore(IHttpContextAccessor httpContextAccessor, ApplicationDbContext applicationDbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<CurrentIdentityData?> GetCurrentIdentityOrDefault()
    {
        if (_currentIdentityData != null) return _currentIdentityData;

        var identityUserIdClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == AuthenticationClaims.IdentityUserId);
        if (identityUserIdClaim is null) return null;

        var identityUserId = Guid.Parse(identityUserIdClaim.Value);

        var identityUser = await _applicationDbContext.Users.FirstAsync(u => u.Id == identityUserId);

        _currentIdentityData = new CurrentIdentityData(identityUser.Id);

        return _currentIdentityData;
    }

    public async Task<CurrentIdentityData> GetCurrentIdentity()
    {
        var currentIdentity = await GetCurrentIdentityOrDefault();
        if (currentIdentity is null) throw new UnauthorizedAccessException();

        return currentIdentity;
    }
}