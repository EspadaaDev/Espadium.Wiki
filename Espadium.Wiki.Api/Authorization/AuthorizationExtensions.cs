using Espadium.Wiki.Application.Services;
using Espadium.Wiki.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Espadium.Wiki.Api.Authorization
{
    public static class AuthorizationExtensions
    {
        public static RouteHandlerBuilder RequireSpaceRole(this RouteHandlerBuilder builder, Guid spaceId, SpaceRole role)
        {
            return builder.AddEndpointFilter(new SpaceRoleFilter(spaceId, role));
        }

        public static RouteHandlerBuilder RequirePageEdit(this RouteHandlerBuilder builder, Guid pageId)
        {
            return builder.AddEndpointFilter(new PageEditFilter(pageId));
        }
    }

    internal class SpaceRoleFilter : IEndpointFilter
    {
        private readonly Guid _spaceId;
        private readonly SpaceRole _role;

        public SpaceRoleFilter(Guid spaceId, SpaceRole role)
        {
            _spaceId = spaceId;
            _role = role;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;
            var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Forbid();

            var perms = http.RequestServices.GetRequiredService<IPermissionService>();
            if (!await perms.HasSpaceRoleAsync(userId, _spaceId, _role)) return Results.Forbid();
            return await next(context);
        }
    }

    internal class PageEditFilter : IEndpointFilter
    {
        private readonly Guid _pageId;

        public PageEditFilter(Guid pageId)
        {
            _pageId = pageId;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;
            var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Forbid();

            var perms = http.RequestServices.GetRequiredService<IPermissionService>();
            if (!await perms.CanEditPageAsync(userId, _pageId)) return Results.Forbid();
            return await next(context);
        }
    }
}

