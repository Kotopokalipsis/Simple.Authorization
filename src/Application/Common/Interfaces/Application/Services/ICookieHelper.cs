namespace Application.Common.Interfaces.Application.Services;

public interface ICookieHelper
{
    void SetRefreshTokenInCookie(string refreshToken);
    string GetRefreshTokenFromCookie();
}