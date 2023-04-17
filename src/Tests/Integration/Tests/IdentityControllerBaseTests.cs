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
        public async Task GetTokenFlow_SuccessTest()
        {
            await _initialFixture.InitialTest();
        }

        // [Theory, Order(2)]
        // [InlineData("/api/identity/login")]
        // public async Task Login_SuccessTest(string url)
        // {
        //     var client = await _initialFixture.InitialRegistrationTest();
        //     
        //     var requestCommand = new LoginCommand
        //     {
        //         Email = "testintegration@gmail.com",
        //         Password = "qwer1296753"
        //     };
        //     
        //     var response = await client.PostAsJsonAsync(url, requestCommand);
        //     response.StatusCode.Should().Be(HttpStatusCode.OK);
        //     
        //     var refreshTokenKayValueArray = _initialFixture.GetRefreshTokenKayValueArray(response);
        //     _initialFixture.RefreshToken = refreshTokenKayValueArray[1];
        //     
        //     var content = await response.Content.ReadAsStringAsync();
        //     var resultCommand = _initialFixture.DeserializeResponse<BaseResponse<Token>>(content);
        //     
        //     _initialFixture.AssertBaseResponse(resultCommand, 200);
        // }
        //
        // [Theory, Order(3)]
        // [InlineData("/api/identity/token/refresh")]
        // public async Task GetRefreshToken_SuccessTest(string url)
        // {
        //     await Login_SuccessTest("/api/identity/login");
        //     var client = _initialFixture.Client;
        //
        //     var response = await client.GetAsync(url);
        //     
        //     response.StatusCode.Should().Be(HttpStatusCode.OK);
        //     
        //     var content = await response.Content.ReadAsStringAsync();
        //     var resultCommand = _initialFixture.DeserializeResponse<BaseResponse<Token>>(content);
        //     
        //     _initialFixture.AssertBaseResponse(resultCommand, 200);
        //     
        //     resultCommand.Data.AccessToken.Should().NotBeEmpty().And.NotBeNull();
        //
        //     _initialFixture.AccessToken = resultCommand.Data.AccessToken;
        // }
    }
}