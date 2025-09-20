using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hn.Api.Models;
using Hn.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hn.Api.Controllers
{
    [ApiController]
    [Route("api/hackernews")]
    public class HackerNewsController(IHackerNewsService hn) : ControllerBase
    {
        [HttpGet("newest")]
        public async Task<ActionResult<IReadOnlyList<int>>> GetNewest(CancellationToken ct = default)
        {
            return Ok(await hn.GetNewestStoriesIdsAsync(ct));
        }

        [HttpGet("newest/page")]
        public async Task<ActionResult<PagedResult<ItemDto>>> GetNewestPage(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var result = await hn.GetNewestPageAsync(page, pageSize, search, ct);
            return Ok(result);
        }

        [HttpGet("item/{id}")]
        public async Task<ActionResult<ItemDto>> GetItem([FromRoute] int id, CancellationToken ct = default)
        {
            var item = await hn.GetItemAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }
    }
}