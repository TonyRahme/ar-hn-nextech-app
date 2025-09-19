using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hn.Api.Services
{
    public interface IHackerNewsService
    {
        Task<IReadOnlyList<int>> GetNewestStoriesAsync();
    }
}