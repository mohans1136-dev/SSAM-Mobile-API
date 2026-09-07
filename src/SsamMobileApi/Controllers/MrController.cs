using Asp.Versioning;
using SsamMobileApi.Models;
using SsamMobileApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SsamMobileApi.Controllers;

/// <summary>Read access to material requests (MTL.MR) and their lines (MTL.MRDetail).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize]
public class MrController(IMrService service) : ControllerBase
{
    /// <summary>List MR headers (paged, newest first). All filters optional.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MrListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MrListItemDto>>> Get(
        [FromQuery] MrQuery query, CancellationToken ct)
        => Ok(await service.GetAsync(query, ct));

    /// <summary>Get one MR header with all its detail lines.</summary>
    [HttpGet("{mrId:int}")]
    [ProducesResponseType(typeof(MrDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MrDto>> GetById(int mrId, CancellationToken ct)
    {
        var mr = await service.GetByIdAsync(mrId, ct);
        return mr is null ? NotFound() : Ok(mr);
    }

    /// <summary>Get just the detail lines for one MR.</summary>
    [HttpGet("{mrId:int}/details")]
    [ProducesResponseType(typeof(IReadOnlyList<MrDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MrDetailDto>>> GetDetails(int mrId, CancellationToken ct)
        => Ok(await service.GetDetailsAsync(mrId, ct));
}
