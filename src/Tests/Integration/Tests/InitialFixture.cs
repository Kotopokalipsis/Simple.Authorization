using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Responses;
using Application.Users.Commands;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Integration.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Web;

namespace Integration.Tests
{
    public class InitialFixture
    {
        public HttpClient Client { get; set; }
        public ApplicationContext Context { get; }
        public RegistrationCommand AuthUserData { get; } = new()
        {
            UserName = "TestUser",
            Email = "testintegration@gmail.com",
            Password = "qwer1296753"
        };
        public Token AuthToken { get; set; }
        private const string RegistrationUrl = "/api/identity/new";
        private WebApplicationFactory<Startup> Factory { get; }
        
        public InitialFixture()
        {
            Factory = new CustomApplicationFactory<Startup>();
            Context = Factory.Services.GetService(typeof(ApplicationContext)) as ApplicationContext;
            Client = Factory.CreateClient();
        }

        public async Task<HttpClient> InitialRegistrationTest()
        {
            if (AuthToken != null)
                return Client;

            var response = await Client.PostAsJsonAsync(RegistrationUrl, AuthUserData);
            var testUser = await Context.Users.FirstOrDefaultAsync(x => x.Email == AuthUserData.Email && x.UserName == AuthUserData.UserName);
                
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            testUser.Email.Should().Be(AuthUserData.Email);
            testUser.UserName.Should().Be(AuthUserData.UserName);
            
            var content = await response.Content.ReadAsStringAsync();
            var resultCommand = DeserializeResponse<BaseResponse<Token>>(content);

            AssertBaseResponse(resultCommand, 201);
           
            resultCommand.Data.AccessToken.Should().NotBeEmpty().And.NotBeNull();
            resultCommand.Data.RefreshToken.Should().NotBeEmpty().And.NotBeNull();

            AuthToken = resultCommand.Data;

            return Client;
        }

        public TValue DeserializeResponse<TValue>(string jsonString)
        {
            return JsonSerializer.Deserialize<TValue>(jsonString, new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});
        }
        
        public static string SerializeResponse<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});
        }

        public void AssertBaseResponse<TValue>(IBaseResponse<TValue> value, int statusCode) where TValue : class
        {
            value.Should().NotBeNull();
            value.Data.Should().NotBeNull();
            value.StatusCode.Should().Be(statusCode);
        }
    }
}