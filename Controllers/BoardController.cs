using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using velcro.Hubs;
using velcro.Models.DTOs;
using velcro.Services.Interfaces;

namespace velcro.Controllers;

[ApiController]
[Route("api/boards")]
[Authorize]
public class BoardController : ControllerBase
{
    private readonly IBoardService _boards;
    private readonly IListService _lists;
    private readonly IHubContext<BoardHub> _hub;

    public BoardController(IBoardService boards, IListService lists, IHubContext<BoardHub> hub)
    {
        _boards = boards;
        _lists = lists;
        _hub = hub;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetByWorkspace(Guid workspaceId)
    {
        try { return Ok(await _boards.GetBoardsByWorkspaceAsync(workspaceId, UserId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("{id}/lists")]
    public async Task<IActionResult> GetLists(Guid id)
    {
        try { return Ok(await _lists.GetListsByBoardAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _boards.GetBoardAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
    {
        try
        {
            var result = await _boards.CreateBoardAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardRequest request)
    {
        try
        {
            var result = await _boards.UpdateBoardAsync(id, request, UserId);
            await _hub.Clients.Group($"board:{id}").SendAsync("BoardUpdated", result);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _hub.Clients.Group($"board:{id}").SendAsync("BoardDeleted", id);
            await _boards.DeleteBoardAsync(id, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
