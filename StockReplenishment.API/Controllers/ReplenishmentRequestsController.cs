using Microsoft.AspNetCore.Mvc;
using StockReplenishment.Application.DTOs.Common;
using StockReplenishment.Application.DTOs.ReplenishmentRequests;
using StockReplenishment.Application.DTOs.StockLocations;
using StockReplenishment.Application.Interfaces;

namespace StockReplenishment.Api.Controllers;

[ApiController]
[Route("api/replenishment-requests")]
public class ReplenishmentRequestsController : ControllerBase
{
    private readonly IReplenishmentRequestService _service;

    public ReplenishmentRequestsController(
        IReplenishmentRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResultDto<ReplenishmentRequestListDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? status,
        [FromQuery] int? priority,
        [FromQuery] int? locationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPagedAsync(
            status,
            priority,
            locationId,
            page,
            pageSize);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ReplenishmentRequestDetailsDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ReplenishmentRequestDetailsDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReplenishmentRequestDto request)
    {
        var result = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ReplenishmentRequestDetailsDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReplenishmentRequestDto request)
    {
        var result = await _service.UpdateAsync(
            id,
            request);

        return Ok(result);
    }

    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(int id)
    {
        await _service.SubmitAsync(id);

        return Accepted(new
        {
            message =
                "Request submitted. Stock validation is in progress.",
            requestId = id
        });
    }

    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int id)
    {
        await _service.ApproveAsync(id);

        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] RejectReplenishmentRequestDto request)
    {
        await _service.RejectAsync(
            id,
            request);

        return NoContent();
    }

    [HttpPost("{id:int}/fulfill")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Fulfill(
        int id,
        [FromBody] FulfillReplenishmentRequestDto request)
    {
        await _service.FulfillAsync(
            id,
            request);

        return NoContent();
    }

    [HttpGet("locations")]
    public async Task<ActionResult<List<StockLocationDto>>>
    GetStockLocations()
    {
        var locations =
            await _service.GetStockLocationsAsync();

        return Ok(locations);
    }
}