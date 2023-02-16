using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Attributes;

public class RefreshTokenRequirementAttribute : TypeFilterAttribute
{
    public RefreshTokenRequirementAttribute() : base(typeof(RefreshTokenRequirementFilter))
    {
    }
}

public class RefreshTokenRequirementFilter : IAuthorizationFilter
{
    public RefreshTokenRequirementFilter()
    {
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey("RefreshToken"))
        {
            context.Result = new ForbidResult();
        }
    }
}