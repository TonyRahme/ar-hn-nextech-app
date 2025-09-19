using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hn.Api.Services
{
    public sealed class HackerNewsService(HttpClient http) : IHackerNewsService
    {
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<int>> GetNewestStoriesAsync()
        {
            var ids = await _http.GetFromJsonAsync<int[]>("/v0/newstories.json");
            return ids ?? Array.Empty<int>();
        }
    }
}