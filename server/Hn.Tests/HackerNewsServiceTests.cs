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

            var result = await client.GetNewestStoriesIdsAsync(CancellationToken.None);

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

        [Fact]
        public async Task When_NonSearch_Paging_Should_Fetch_All_And_HasNext()
        {
            //Arrange
            var mock = new MockHttpMessageHandler();
            mock.Expect($"{BASE_URL}newstories.json").Respond("application/json", "[5, 6, 7]");

            mock.Expect($"{BASE_URL}item/5.json")
                .Respond("application/json", """{"id":5,"title":"A"}""");
            mock.Expect($"{BASE_URL}item/6.json")
                .Respond("application/json", """{"id":6,"title":"B"}""");

            //Act

            var service = CreateService(mock, out var cache);
            var page = await service.GetNewestPageAsync(page: 1, pageSize: 2, search: null, ct: CancellationToken.None);
            page.Items.Select(i => i.Id).Should().Equal(5, 6);
            page.TotalCount.Should().Be(3);
            page.HasNext.Should().BeTrue();

            mock.VerifyNoOutstandingExpectation();
            cache.Dispose();
        }

        [Fact]
        public async Task When_Search_Title_And_Text_Should_Strip_CaseInsensitive()
        {
            var mock = new MockHttpMessageHandler();
            //Arrange
            mock.Expect($"{BASE_URL}newstories.json")
            .Respond("application/json", "[101,102,103,104]");

            // 101: matches both tokens (split across title/text)
            mock.Expect($"{BASE_URL}item/101.json")
                .Respond("application/json",
                    """{"id":101,"title":"foofoo guide","text":"Intro to <em>LABUBU</em>"}""");

            // 102: only "foofoo"
            mock.Expect($"{BASE_URL}item/102.json")
                .Respond("application/json",
                    """{"id":102,"title":"about foofoo"}""");

            // 103: only "labubu"
            mock.Expect($"{BASE_URL}item/103.json")
                .Respond("application/json",
                    """{"id":103,"text":"learning labubu today"}""");

            // 104: unrelated
            mock.Expect($"{BASE_URL}item/104.json")
                .Respond("application/json",
                    """{"id":104,"title":"skibidi"}""");

            var service = CreateService(mock, out var cache);

            var page = await service.GetNewestPageAsync(page: 1, pageSize: 10, search: "foofoo labubu", ct: CancellationToken.None);
            page.TotalCount.Should().Be(1); //should only contain id 101
            mock.VerifyNoOutstandingExpectation();
            cache.Dispose();
        }

        [Fact]
        public async Task When_Search_Pagination_Works_Should_Preserve_Order()
        {
            var mock = new MockHttpMessageHandler();
            mock.Expect($"{BASE_URL}newstories.json")
            .Respond("application/json", "[201,202,203,204]");

            mock.Expect($"{BASE_URL}item/201.json").Respond("application/json", """{"id":201,"title":"foo a"}""");
            mock.Expect($"{BASE_URL}item/202.json").Respond("application/json", """{"id":202,"title":"foo b"}""");
            mock.Expect($"{BASE_URL}item/203.json").Respond("application/json", """{"id":203,"title":"foo c"}""");
            mock.Expect($"{BASE_URL}item/204.json").Respond("application/json", """{"id":204,"title":"bar"}""");


            var service = CreateService(mock, out var cache);

            var page1 = await service.GetNewestPageAsync(1, 2, "foo", CancellationToken.None);
            page1.TotalCount.Should().Be(3);
            page1.HasNext.Should().BeTrue();
            page1.Items.Select(i => i.Id).Should().Equal(201, 202);

            var page2 = await service.GetNewestPageAsync(2, 2, "foo", CancellationToken.None);
            page2.TotalCount.Should().Be(3);
            page2.HasNext.Should().BeFalse();
            page2.Items.Select(i => i.Id).Should().Equal(203);

            mock.VerifyNoOutstandingExpectation();
            cache.Dispose();
        }

        [Fact]
    public async Task When_Empty_Newstories_Should_Return_Empty_Page()
    {
        var mock = new MockHttpMessageHandler();

        mock.Expect($"{BASE_URL}newstories.json")
            .Respond("application/json", "[]");

        var svc = CreateService(mock, out var cache);

        var page = await svc.GetNewestPageAsync(1, 20, "anything", CancellationToken.None);

        page.TotalCount.Should().Be(0);
        page.Items.Should().BeEmpty();
        page.HasNext.Should().BeFalse();

        mock.VerifyNoOutstandingExpectation();
        cache.Dispose();
    }
    }
}