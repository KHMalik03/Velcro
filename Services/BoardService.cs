using Microsoft.EntityFrameworkCore;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;

namespace velcro.Services;

public class BoardService : IBoardService
{
    private readonly ApplicationDbContext _db;

    public BoardService(ApplicationDbContext db) => _db = db;

    public async Task<List<BoardSummaryDto>> GetBoardsByWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        if (!await _db.WorkspaceMembers.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");

        return await _db.Boards
            .Where(b => b.WorkspaceId == workspaceId && !b.IsArchived)
            .Select(b => new BoardSummaryDto(b.Id, b.WorkspaceId, b.Name, b.Description, b.BackgroundColor, b.IsArchived))
            .ToListAsync();
    }

    public async Task<BoardDto> GetBoardAsync(Guid id, Guid userId)
    {
        var board = await _db.Boards
            .Include(b => b.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Board introuvable.");

        if (!board.Members.Any(m => m.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");

        return ToDto(board);
    }

    public async Task<BoardDto> CreateBoardAsync(CreateBoardRequest request, Guid userId)
    {
        if (!await _db.WorkspaceMembers.AnyAsync(wm => wm.WorkspaceId == request.WorkspaceId && wm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");

        var board = new Board
        {
            Id = Guid.NewGuid(),
            WorkspaceId = request.WorkspaceId,
            Name = request.Name,
            Description = request.Description,
            BackgroundColor = request.BackgroundColor ?? "#0079BF",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Boards.Add(board);
        _db.BoardMembers.Add(new BoardMember
        {
            BoardId = board.Id,
            UserId = userId,
            Role = BoardRole.Admin,
            JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await GetBoardAsync(board.Id, userId);
    }

    public async Task<BoardDto> UpdateBoardAsync(Guid id, UpdateBoardRequest request, Guid userId)
    {
        var board = await _db.Boards
            .Include(b => b.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Board introuvable.");

        await EnsureAdminAsync(id, userId);

        if (request.Name != null) board.Name = request.Name;
        if (request.Description != null) board.Description = request.Description;
        if (request.BackgroundColor != null) board.BackgroundColor = request.BackgroundColor;
        if (request.IsArchived.HasValue) board.IsArchived = request.IsArchived.Value;
        board.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(board);
    }

    public async Task DeleteBoardAsync(Guid id, Guid userId)
    {
        var board = await _db.Boards.FindAsync(id)
            ?? throw new KeyNotFoundException("Board introuvable.");
        await EnsureAdminAsync(id, userId);
        _db.Boards.Remove(board);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureAdminAsync(Guid boardId, Guid userId)
    {
        var member = await _db.BoardMembers
            .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
        if (member == null || member.Role != BoardRole.Admin)
            throw new UnauthorizedAccessException("Permissions insuffisantes.");
    }

    private static BoardDto ToDto(Board b) =>
        new(b.Id, b.WorkspaceId, b.Name, b.Description, b.BackgroundColor, b.IsArchived,
            b.Members.Select(m => new BoardMemberDto(m.UserId, m.User.Username, m.Role.ToString())).ToList(),
            b.CreatedAt, b.UpdatedAt);
}
