using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hn.Api.Models;
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
        private static readonly TimeSpan ItemTtl      = TimeSpan.FromMinutes(10);


        public HackerNewsService(HttpClient http, IMemoryCache cache, ILogger<HackerNewsService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ItemDto?> GetItemAsync(int id, CancellationToken ct)
        {
            return await GetCachedAsync(
                key: "hn:item:{id}", ttl: ItemTtl,
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

        public async Task<IReadOnlyList<int>> GetNewestStoriesAsync(CancellationToken ct)
        {
            return await GetCachedAsync<IReadOnlyList<int>>(key: "hn:newest", ttl: NewestIdsTtl, factory: async token =>
            {
                var ids = await _http.GetFromJsonAsync<int[]>("/v0/newstories.json", ct);
                return (IReadOnlyList<int>)(ids ?? Array.Empty<int>());
            },
            ct: ct);
        }


        //Helper functions

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