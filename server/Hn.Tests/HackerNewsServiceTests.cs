using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hn.Api.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace Hn.Tests
{
    public class HackerNewsServiceTests
    {
        [Fact]
        public async Task When_GetNewestStoriesAsync_Returns_Ids()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Respond("application/json", "[1,2,3]");

            var http = new HttpClient(mockHttp) { BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/") };
            var client = new HackerNewsService(http);

            var result = await client.GetNewestStoriesAsync();

            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task GetItemAsync_404_Returns_Null()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/999.json")
                    .Respond(HttpStatusCode.NotFound);

            var http = new HttpClient(mockHttp) { BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/") };
            var client = new HackerNewsService(http);

            var item = await client.GetItemAsync(999);

            item.Should().BeNull();
        }
    }
}