using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hn.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hn.Api.Controllers
{
    [ApiController]
    [Route("api/hackernews")]
    public class HackerNewsController(IHackerNewsService hn) : ControllerBase
    {
        [HttpGet("latest")]
        public async Task<ActionResult<IReadOnlyList<int>>> GetLatest()
        {
            return Ok(await hn.GetNewestStoriesAsync());
        }
    }
}