using Asp.Versioning;
using ConnectorDistributor.Api.Models;
using ConnectorDistributor.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConnectorDistributor.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize] // requires a valid Entra ID bearer token (set globally too, kept here for clarity)
public class DistributorsController(IDistributorService service) : ControllerBase
{
    /// <summary>List distributors (paged, optional search).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DistributorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DistributorDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await service.GetAsync(page, pageSize, search, ct));

    /// <summary>Get a single distributor by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DistributorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DistributorDto>> GetById(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a distributor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DistributorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DistributorDto>> Create([FromBody] CreateDistributorRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1.0" }, created);
    }

    /// <summary>Replace an existing distributor.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDistributorRequest request, CancellationToken ct)
        => await service.UpdateAsync(id, request, ct) ? NoContent() : NotFound();

    /// <summary>Delete a distributor.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
