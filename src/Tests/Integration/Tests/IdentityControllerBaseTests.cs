using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Responses;
using Application.Users.Commands;
using Domain.ValueObjects;
using FluentAssertions;
using Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Web;
using Xunit;
using Xunit.Extensions.Ordering;

[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]

namespace Integration.Tests
{
    public class IdentityControllerBaseTests : IClassFixture<InitialFixture>
    {
        private readonly InitialFixture _initialFixture;

        public IdentityControllerBaseTests(InitialFixture initialFixture)
        {
            _initialFixture = initialFixture;
        }

        [Fact, Order(1)]
        public async Task Registration_SuccessTest()
        {
            await _initialFixture.InitialRegistrationTest();
        }

        [Theory, Order(2)]
        [InlineData("/api/identity/login")]
        public async Task Login_SuccessTest(string url)
        {
            var client = await _initialFixture.InitialRegistrationTest();
            
            var requestCommand = new LoginCommand
            {
                Email = "testintegration@gmail.com",
                Password = "qwer1296753"
            };
            
            var response = await client.PostAsJsonAsync(url, requestCommand);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = _initialFixture.DeserializeResponse<BaseResponse<Token>>(content);
            
            _initialFixture.AssertBaseResponse(resultCommand, 200);
           
            resultCommand.Data.AccessToken.Should().NotBeEmpty().And.NotBeNull();
            resultCommand.Data.RefreshToken.Should().NotBeEmpty().And.NotBeNull();

            var userRefreshToken = await _initialFixture.Context.UserRefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == resultCommand.Data.RefreshToken);
            var refreshTokenInBlacklist = await _initialFixture.Context.RefreshTokenBlacklists.AnyAsync(x => x.RefreshToken == resultCommand.Data.RefreshToken);

            userRefreshToken.Should().NotBeNull();
            refreshTokenInBlacklist.Should().BeTrue();

            _initialFixture.AuthToken = resultCommand.Data;
        }
        
        [Theory, Order(3)]
        [InlineData("/api/identity/token/refresh")]
        public async Task RefreshToken_SuccessTest(string url)
        {
            var client = await _initialFixture.InitialRegistrationTest();
            
            var requestCommand = new RefreshTokenCommand()
            {
                RefreshToken = _initialFixture.AuthToken.RefreshToken
            };
            
            var response = await client.PostAsJsonAsync(url, requestCommand);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = _initialFixture.DeserializeResponse<BaseResponse<Token>>(content);
            
            _initialFixture.AssertBaseResponse(resultCommand, 200);
           
            resultCommand.Data.AccessToken.Should().NotBeEmpty().And.NotBeNull();
            resultCommand.Data.RefreshToken.Should().NotBeEmpty().And.NotBeNull();
            
            var refreshTokenOnUser = await _initialFixture.Context.UserRefreshTokens.AnyAsync(x => x.RefreshToken == requestCommand.RefreshToken);
            var refreshTokenInBlacklist = await _initialFixture.Context.RefreshTokenBlacklists.AnyAsync(x => x.RefreshToken == resultCommand.Data.RefreshToken);

            refreshTokenOnUser.Should().BeFalse();
            refreshTokenInBlacklist.Should().BeTrue();
            
            _initialFixture.AuthToken = resultCommand.Data;
        }
    }
}