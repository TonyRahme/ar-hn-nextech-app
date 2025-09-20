using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hn.Api.Models;

namespace Hn.Api.Services
{
    public sealed class HackerNewsService(HttpClient http) : IHackerNewsService
    {
        private readonly HttpClient _http = http;

        public async Task<ItemDto?> GetItemAsync(int id)
        {
            using var result = await _http.GetAsync($"/v0/item/{id}.json");
            if (result.StatusCode == HttpStatusCode.NotFound)
                return null;
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<ItemDto>();
        }

        public async Task<IReadOnlyList<int>> GetNewestStoriesAsync()
        {
            var ids = await _http.GetFromJsonAsync<int[]>("/v0/newstories.json");
            return ids ?? Array.Empty<int>();
        }

        
    }
}