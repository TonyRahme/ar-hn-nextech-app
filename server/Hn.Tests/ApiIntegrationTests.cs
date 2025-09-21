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
using Hn.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hn.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private const string BASE_URL = "https://hacker-news.firebaseio.com/v0/";

        static string ObjToJson<T>(T objToJson) => JsonSerializer.Serialize(
            objToJson,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });


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
            mockHttp.When($"{BASE_URL}newstories.json")
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
                    c.BaseAddress = new Uri($"{BASE_URL}");
                })
                .ConfigurePrimaryHttpMessageHandler(() => mockHttp);
            });
        }).CreateClient();

            var resp = await client.GetAsync("/hackernews/newest");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var ids = await resp.Content.ReadFromJsonAsync<int[]>();
            ids.Should().Equal(10, 11);
        }

        [Fact]
        public async Task When_GetComments_then_Returns_Ok_With_CommentItem_List()
        {
            var mockHttp = new MockHttpMessageHandler();
            const string BASE_URL = "https://hacker-news.firebaseio.com/v0/";

            // parent story with kids
            mockHttp.When($"{BASE_URL}item/1.json")
                    .Respond("application/json", """{"id":1,"title":"foo a","kids":[201,202]}""");

            mockHttp.When($"{BASE_URL}item/201.json")
                    .Respond("application/json", """{"id":201,"title":"foo comment a"}""");

            mockHttp.When($"{BASE_URL}item/202.json")
                    .Respond("application/json", """{"id":202,"title":"foo comment b"}""");

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var typed = services.FirstOrDefault(s => s.ServiceType == typeof(IHackerNewsService));
                    if (typed is not null) services.Remove(typed);

                    services.AddHttpClient<IHackerNewsService, HackerNewsService>(c =>
                    {
                        c.BaseAddress = new Uri(BASE_URL);
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => mockHttp);
                });
            }).CreateClient();

            var resp = await client.GetAsync("/hackernews/items/1/comments");

            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var comments = await resp.Content.ReadFromJsonAsync<ItemDto[]>();
            comments.Should().NotBeNull();
            comments!.Select(c => c.Id).Should().Equal(201, 202);

        }
    }
}