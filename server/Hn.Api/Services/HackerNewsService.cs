using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hn.Api.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace Hn.Api.Services
{
    public sealed class HackerNewsService : IHackerNewsService
    {


        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HackerNewsService> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private static readonly TimeSpan NewestIdsTtl = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ItemTtl = TimeSpan.FromMinutes(10);


        public HackerNewsService(HttpClient http, IMemoryCache cache, ILogger<HackerNewsService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ItemDto?> GetItemAsync(int id, CancellationToken ct = default)
        {
            return await GetCachedAsync(
                key: $"hn:item:{id}", ttl: ItemTtl,
                factory: async token =>
                {
                    using var result = await _http.GetAsync($"/v0/item/{id}.json", ct);
                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    result.EnsureSuccessStatusCode();
                    return await result.Content.ReadFromJsonAsync<ItemDto>();
                },
                ct);
        }

        public async Task<IReadOnlyList<int>> GetNewestStoriesIdsAsync(CancellationToken ct = default)
        {
            return await GetCachedAsync<IReadOnlyList<int>>(key: "hn:newest", ttl: NewestIdsTtl, factory: async token =>
            {
                var ids = await _http.GetFromJsonAsync<int[]>("/v0/newstories.json", ct);
                return (IReadOnlyList<int>)(ids ?? Array.Empty<int>());
            },
            ct: ct);
        }

        public async Task<PagedResult<ItemDto>> GetNewestPageAsync(int page, int pageSize, string? search, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;
            var ids = await GetNewestStoriesIdsAsync();
            //No search
            if (string.IsNullOrWhiteSpace(search))
            {
                var skip = (page - 1) * pageSize;
                var slice = ids.Skip(skip).Take(pageSize).ToArray();
                var items = await FetchItems(slice, ct);
                return new PagedResult<ItemDto>(items, page, pageSize, ids.Count, skip + items.Count < ids.Count);
            }

            //with search case-insensitive
            var tokens = Tokenize(search!);
            var matches = new List<ItemDto>();
            var totalMatches = 0;

            foreach (var id in ids)
            {
                var item = await GetItemAsync(id, ct);
                if (item is null) continue;

                var title = item.Title ?? string.Empty;
                var text = StripTags(item.Text ?? string.Empty);

                if (MatchesAllTokens(title, text, tokens))
                {
                    totalMatches++;
                    var offset = (page - 1) * pageSize;
                    if (totalMatches > offset && matches.Count < pageSize) matches.Add(item);
                }
            }

            var hasNext = (page * pageSize) < totalMatches;

            return new PagedResult<ItemDto>(matches, page, pageSize, totalMatches, hasNext);
        }


        //Helper functions

        private async Task<List<ItemDto>> FetchItems(int[] ids, CancellationToken ct)
        {
            const int maxConcurrency = 8;
            using var gate = new SemaphoreSlim(maxConcurrency);
            var tasks = ids.Select(async id =>
            {
                await gate.WaitAsync(ct);
                try { return await GetItemAsync(id, ct); }
                finally { gate.Release(); }
            });
            var results = await Task.WhenAll(tasks);
            return results.Where(x => x is not null)!.Cast<ItemDto>().ToList();
        }

        private static string[] Tokenize(string q) =>
    q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static readonly Regex _tagRx = new("<[^>]+>", RegexOptions.Compiled);
        private static string StripTags(string html) => _tagRx.Replace(html, " ");

        private static bool MatchesAllTokens(string title, string text, string[] tokens)
        {
            foreach (var t in tokens)
            {
                if (!ContainsIgnoreCase(title, t) && !ContainsIgnoreCase(text, t))
                    return false;
            }
            return true;
        }
        private static bool ContainsIgnoreCase(string s, string term) =>
            s.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;


        private async Task<T> GetCachedAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct)
        {
            if (_cache.TryGetValue<T>(key, out var cached))
                return cached!;

            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                if (_cache.TryGetValue<T>(key, out cached))
                    return cached!;

                var value = await factory(ct);

                _cache.Set(key, value!, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });

                _logger.LogDebug("Cached {Key} for {Seconds}s", key, ttl.TotalSeconds);
                return value!;
            }
            finally
            {
                gate.Release();
                if (gate.CurrentCount == 1) _locks.TryRemove(key, out _);
            }
        }

    }
}