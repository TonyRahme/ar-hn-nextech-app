using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hn.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Hn.Tests
{
    public class HackerNewsServiceTests
    {

        private const string BASE_URL = "https://hacker-news.firebaseio.com/v0/";

        private static HackerNewsService CreateService(HttpMessageHandler handler, out IMemoryCache cache)
        {
            var http = new HttpClient(handler) { BaseAddress = new Uri(BASE_URL) };
            cache = new MemoryCache(new MemoryCacheOptions());
            return new HackerNewsService(http, cache, NullLogger<HackerNewsService>.Instance);
        }

        [Fact]
        public async Task When_GetNewestStoriesAsync_Returns_Ids()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{BASE_URL}newstories.json")
                    .Respond("application/json", "[1,2,3]");

            var client = CreateService(mockHttp, out var cache);

            var result = await client.GetNewestStoriesAsync(CancellationToken.None);

            result.Should().Equal(1, 2, 3);

            cache.Dispose();
        }

        [Fact]
        public async Task When_GetItemAsync_Returns_404_Then_Return_Null()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{BASE_URL}item/999.json")
                    .Respond(HttpStatusCode.NotFound);

            var client = CreateService(mockHttp, out var cache);

            var item = await client.GetItemAsync(999, CancellationToken.None);

            item.Should().BeNull();

            cache.Dispose();
        }
    }
}