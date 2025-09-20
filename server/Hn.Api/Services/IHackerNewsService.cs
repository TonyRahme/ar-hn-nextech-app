using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hn.Api.Models;

namespace Hn.Api.Services
{
    public interface IHackerNewsService
    {
        Task<IReadOnlyList<int>> GetNewestStoriesIdsAsync(CancellationToken ct = default);
        Task<ItemDto?> GetItemAsync(int id, CancellationToken ct = default);
        Task<PagedResult<ItemDto>> GetNewestPageAsync(int page, int pageSize, string? search, CancellationToken ct = default);
        
    }
}