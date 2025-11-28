using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure;
using System.Security.Claims;

namespace Shop.Authorization;

class ItemOwnerRequirement : IAuthorizationRequirement
{
}

class ItemOwnerHandler : AuthorizationHandler<ItemOwnerRequirement>
{
    private readonly ApplicationContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ItemOwnerHandler(ApplicationContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ItemOwnerRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userIdClaim = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        var routeData = httpContext.GetRouteData();
        var itemIdValue = routeData?.Values["id"]?.ToString();

        if (string.IsNullOrEmpty(itemIdValue) || !int.TryParse(itemIdValue, out var itemId))
        {
            context.Fail();
            return;
        }

        var isOwner = await _context.Items
            .AnyAsync(i => i.Id == itemId && i.SellerId == userId);

        if (isOwner)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

