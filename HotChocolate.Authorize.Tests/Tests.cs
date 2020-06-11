using HotChocolateAuthorizeBug;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HotChocolate.Authorize.Tests
{
    public class Tests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _authTestFactory;

        public Tests(WebApplicationFactory<Startup> authFixture)
        {
            _authTestFactory = authFixture;
        }

        [Fact]
        public async Task Controller_Auth_With_Token_Success()
        {
            var tokenResponse = await RequestTokenAsync();
            var client = _authTestFactory.CreateClient();

            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("/home/index");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Controller_Auth_Without_Token_Fails()
        {
            var client = _authTestFactory.CreateClient();

            var response = await client.GetAsync("/home/index");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GraphQL_Auth_With_Token_Success()
        {
            var tokenResponse = await RequestTokenAsync();
            var client = _authTestFactory.CreateClient();

            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.PostAsync("/graphql",
               new StringContent(JsonConvert.SerializeObject(
                   new { query = "query { secured }" }),
                   Encoding.UTF8,
                   "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain("The current user is not authorized to access this resource", responseContent);
        }

        [Fact]
        public async Task GraphQL_Auth_Without_Token_Fails()
        {
            var client = _authTestFactory.CreateClient();

            var response = await client.PostAsync("/graphql",
               new StringContent(JsonConvert.SerializeObject(
                   new { query = "query { secured }" }),
                   Encoding.UTF8,
                   "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("The current user is not authorized to access this resource", responseContent);
        }

        private async Task<TokenResponse> RequestTokenAsync()
        {
            var client = _authTestFactory.CreateClient();

            var disco = await client.GetDiscoveryDocumentAsync(client.BaseAddress.ToString());
            if (disco.IsError) throw new Exception(disco.Error);

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scope = "IdentityServerApi"
            };

            var response = await client.RequestClientCredentialsTokenAsync(tokenRequest);

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }
    }
}
