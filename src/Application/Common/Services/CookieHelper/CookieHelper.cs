using Application.Common.Interfaces.Application.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Services.CookieHelper;

public class CookieHelper : ICookieHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetRefreshTokenInCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        };
                
        _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
    }

    public string GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];
    }
}