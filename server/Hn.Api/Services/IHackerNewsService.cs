using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hn.Api.Models;

namespace Hn.Api.Services
{
    public interface IHackerNewsService
    {
        Task<IReadOnlyList<int>> GetNewestStoriesAsync(CancellationToken ct);
        Task<ItemDto?> GetItemAsync(int id, CancellationToken ct);
        
    }
}