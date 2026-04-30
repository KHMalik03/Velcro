using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using velcro.Hubs;
using velcro.Models.DTOs;
using velcro.Services.Interfaces;

namespace velcro.Controllers;

[ApiController]
[Route("api/cards")]
[Authorize]
public class CardController : ControllerBase
{
    private readonly ICardService _cards;
    private readonly ICommentService _comments;
    private readonly IHubContext<BoardHub> _hub;

    public CardController(ICardService cards, ICommentService comments, IHubContext<BoardHub> hub)
    {
        _cards = cards;
        _comments = comments;
        _hub = hub;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _cards.GetCardAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
    {
        try
        {
            var result = await _cards.CreateCardAsync(request, UserId);
            await _hub.Clients.Group($"board:{result.BoardId}").SendAsync("CardCreated", result);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCardRequest request)
    {
        try
        {
            var result = await _cards.UpdateCardAsync(id, request, UserId);
            await _hub.Clients.Group($"board:{result.BoardId}").SendAsync("CardUpdated", result);
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
            var boardId = await _cards.DeleteCardAsync(id, UserId);
            await _hub.Clients.Group($"board:{boardId}").SendAsync("CardDeleted", id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPatch("{id}/move")]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveCardRequest request)
    {
        try
        {
            var result = await _cards.MoveCardAsync(id, request, UserId);
            await _hub.Clients.Group($"board:{result.BoardId}").SendAsync("CardMoved", result);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // Comments
    [HttpPost("{cardId}/comments")]
    public async Task<IActionResult> AddComment(Guid cardId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var result = await _comments.AddCommentAsync(cardId, request, UserId);
            await _hub.Clients.Group($"board:{result.BoardId}").SendAsync("CommentAdded", result);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{cardId}/comments/{id}")]
    public async Task<IActionResult> UpdateComment(Guid cardId, Guid id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var result = await _comments.UpdateCommentAsync(cardId, id, request, UserId);
            await _hub.Clients.Group($"board:{result.BoardId}").SendAsync("CommentUpdated", result);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{cardId}/comments/{id}")]
    public async Task<IActionResult> DeleteComment(Guid cardId, Guid id)
    {
        try
        {
            var boardId = await _comments.DeleteCommentAsync(cardId, id, UserId);
            await _hub.Clients.Group($"board:{boardId}").SendAsync("CommentDeleted", new { cardId, commentId = id });
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
