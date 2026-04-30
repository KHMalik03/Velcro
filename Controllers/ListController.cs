using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using velcro.Models.DTOs;
using velcro.Services.Interfaces;

namespace velcro.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize]
public class ListController : ControllerBase
{
    private readonly IListService _lists;

    public ListController(IListService lists) => _lists = lists;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _lists.GetListAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListRequest request)
    {
        try
        {
            var result = await _lists.CreateListAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListRequest request)
    {
        try { return Ok(await _lists.UpdateListAsync(id, request, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { await _lists.DeleteListAsync(id, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("reorder/{boardId}")]
    public async Task<IActionResult> Reorder(Guid boardId, [FromBody] ReorderListsRequest request)
    {
        try { await _lists.ReorderListsAsync(boardId, request, UserId); return NoContent(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
