using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hn.Api.Models
{
    public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNext
);
}