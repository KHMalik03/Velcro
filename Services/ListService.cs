using Microsoft.EntityFrameworkCore;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;
using EntityList = velcro.Models.Entities.List;

namespace velcro.Services;

public class ListService : IListService
{
    private readonly ApplicationDbContext _db;

    public ListService(ApplicationDbContext db) => _db = db;

    public async Task<ListDto> GetListAsync(Guid id, Guid userId)
    {
        var list = await _db.Lists.FindAsync(id)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        await EnsureBoardMemberAsync(list.BoardId, userId);
        return ToDto(list);
    }

    public async Task<ListDto> CreateListAsync(CreateListRequest request, Guid userId)
    {
        await EnsureBoardMemberAsync(request.BoardId, userId);
        var position = await _db.Lists.CountAsync(l => l.BoardId == request.BoardId && !l.IsArchived);
        var list = new EntityList
        {
            Id = Guid.NewGuid(),
            BoardId = request.BoardId,
            Name = request.Name,
            Position = position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Lists.Add(list);
        await _db.SaveChangesAsync();
        return ToDto(list);
    }

    public async Task<ListDto> UpdateListAsync(Guid id, UpdateListRequest request, Guid userId)
    {
        var list = await _db.Lists.FindAsync(id)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        await EnsureBoardMemberAsync(list.BoardId, userId);
        if (request.Name != null) list.Name = request.Name;
        if (request.IsArchived.HasValue) list.IsArchived = request.IsArchived.Value;
        list.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(list);
    }

    public async Task DeleteListAsync(Guid id, Guid userId)
    {
        var list = await _db.Lists.FindAsync(id)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        await EnsureBoardMemberAsync(list.BoardId, userId);
        _db.Lists.Remove(list);
        await _db.SaveChangesAsync();
    }

    public async Task ReorderListsAsync(Guid boardId, ReorderListsRequest request, Guid userId)
    {
        await EnsureBoardMemberAsync(boardId, userId);
        var lists = await _db.Lists.Where(l => l.BoardId == boardId).ToListAsync();
        foreach (var item in request.Lists)
        {
            var list = lists.FirstOrDefault(l => l.Id == item.Id);
            if (list != null) list.Position = item.Position;
        }
        await _db.SaveChangesAsync();
    }

    private async Task EnsureBoardMemberAsync(Guid boardId, Guid userId)
    {
        if (!await _db.BoardMembers.AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");
    }

    private static ListDto ToDto(EntityList l) =>
        new(l.Id, l.BoardId, l.Name, l.Position, l.IsArchived, l.CreatedAt, l.UpdatedAt);
}
