using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using velcro.Models.DTOs;
using velcro.Services.Interfaces;

namespace velcro.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaces;

    public WorkspaceController(IWorkspaceService workspaces) => _workspaces = workspaces;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _workspaces.GetUserWorkspacesAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _workspaces.GetWorkspaceAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
    {
        var result = await _workspaces.CreateWorkspaceAsync(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkspaceRequest request)
    {
        try { return Ok(await _workspaces.UpdateWorkspaceAsync(id, request, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { await _workspaces.DeleteWorkspaceAsync(id, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
