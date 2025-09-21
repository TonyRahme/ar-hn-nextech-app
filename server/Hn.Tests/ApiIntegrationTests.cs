using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RichardSzalay.MockHttp;
using Xunit;
using Hn.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Hn.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;


        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // remove existing typed client registration
                var descriptor = services.First(d =>
                    d.ServiceType == typeof(IHttpClientFactory));
            });
        });
        }

        [Fact]
        public async Task When_GetNewest_Then_Returns_Ok_With_Id_List()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Respond("application/json", "[10, 11]");
            var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // remove existing typed client registration
                var toRemove = services.Where(s =>
                    s.ServiceType == typeof(IHttpClientFactory) ||
                    s.ServiceType == typeof(IHackerNewsService)).ToList();
                foreach (var s in toRemove) services.Remove(s);

                // re-register a deterministic HttpClient + typed client
                services.AddHttpClient<IHackerNewsService, HackerNewsService>(c =>
                {
                    c.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
                })
                .ConfigurePrimaryHttpMessageHandler(() => mockHttp);
            });
        }).CreateClient();

            var resp = await client.GetAsync("/hackernews/newest");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var ids = await resp.Content.ReadFromJsonAsync<int[]>();
            ids.Should().Equal(10, 11);
        }
    }
}