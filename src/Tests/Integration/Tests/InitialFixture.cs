using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Responses;
using Application.Users.Commands;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Integration.Fixtures;
using Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Web;

namespace Integration.Tests
{
    public class InitialFixture
    {
        public HttpClient Client { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        
        private const string RegistrationUrl = "/api/identity/new";
        private const string LoginUrl = "/api/identity/login";
        private const string GetAccessTokenUrl = "/api/identity/token/access";
        
        private WebApplicationFactory<Startup> Factory { get; }
        private ApplicationContext Context { get; }
        private RegistrationCommand AuthUserData { get; } = new()
        {
            UserName = "TestUser",
            Email = "testintegration@gmail.com",
            Password = "qwer1296753"
        };
        
        public InitialFixture()
        {
            Factory = new CustomApplicationFactory<Startup>();
            Context = Factory.Services.GetService(typeof(ApplicationContext)) as ApplicationContext;
            
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            
            Client = Factory.CreateClient();
        }

        public async Task InitialTest()
        {
            await Registration_SuccessTest(RegistrationUrl);
        }

        private async Task Registration_SuccessTest(string url)
        {
            var response = await Client.PostAsJsonAsync(url, AuthUserData);
            var testUser = await Context.Users.FirstOrDefaultAsync(x => x.Email == AuthUserData.Email && x.UserName == AuthUserData.UserName);
                
            testUser.Email.Should().Be(AuthUserData.Email);
            testUser.UserName.Should().Be(AuthUserData.UserName);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var cookie = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            cookie.Should().BeNull();
            
            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = IntegrationsTestsHelper.DeserializeResponse<BaseResponse<Token>>(content);

            IntegrationsTestsHelper.AssertBaseResponse(resultCommand, 201);

            await Login_SuccessTest(LoginUrl);
        }
        
        private async Task Login_SuccessTest(string url)
        {
            var requestCommand = new LoginCommand
            {
                Email = "testintegration@gmail.com",
                Password = "qwer1296753"
            };
            
            var response = await Client.PostAsJsonAsync(url, requestCommand);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            RefreshToken = IntegrationsTestsHelper.GetRefreshTokenFromCookie(response, Client);

            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = IntegrationsTestsHelper.DeserializeResponse<BaseResponse<Token>>(content);
            
            IntegrationsTestsHelper.AssertBaseResponse(resultCommand, 200);

            await GetAccessToken_SuccessTest(GetAccessTokenUrl);
        }
        
        private async Task GetAccessToken_SuccessTest(string url)
        {
            IntegrationsTestsHelper.SetRefreshTokenCookie(Client, RefreshToken);

            var response = await Client.GetAsync(url);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = IntegrationsTestsHelper.DeserializeResponse<BaseResponse<Token>>(content);
            
            IntegrationsTestsHelper.AssertBaseResponse(resultCommand, 200);
            
            resultCommand.Data.AccessToken.Should().NotBeEmpty().And.NotBeNull();

            AccessToken = resultCommand.Data.AccessToken;
        }
    }
}