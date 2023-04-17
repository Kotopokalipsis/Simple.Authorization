using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Application.Common.Interfaces.Application.Responses;
using FluentAssertions;

namespace Integration.Helpers;

public static class IntegrationsTestsHelper
{
    public static TValue DeserializeResponse<TValue>(string jsonString)
    {
        return JsonSerializer.Deserialize<TValue>(jsonString, new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});
    }
        
    public static string SerializeResponse<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});
    }

    public static void AssertBaseResponse<TValue>(IBaseResponse<TValue> value, int statusCode) where TValue : class
    {
        value.Should().NotBeNull();
        value.StatusCode.Should().Be(statusCode);
    }

    public static string GetRefreshTokenFromCookie(HttpResponseMessage response, HttpClient client)
    {
        var cookies = new CookieContainer();

        var cookiesHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        
        cookiesHeader.Should().NotBeNull();
        cookiesHeader!.Split(";").FirstOrDefault().Should().NotBeNull();
        
        cookies.SetCookies(client.BaseAddress!, cookiesHeader!.Split(";").FirstOrDefault()!);

        var refreshTokenCookie = cookies.GetCookies(client.BaseAddress!).FirstOrDefault(cookie => cookie.Name == "RefreshToken");
        refreshTokenCookie.Should().NotBeNull();
            
        return refreshTokenCookie!.Value;
    }

    public static void SetRefreshTokenCookie(HttpClient client, string refreshToken)
    {
        var cookies = new CookieContainer();
        cookies.Add(client.BaseAddress!, new Cookie("RefreshToken", refreshToken));
        client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(client.BaseAddress));
    }
}