using Microsoft.EntityFrameworkCore;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;

namespace velcro.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly ApplicationDbContext _db;

    public WorkspaceService(ApplicationDbContext db) => _db = db;

    public async Task<List<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId)
    {
        return await _db.WorkspaceMembers
            .Where(wm => wm.UserId == userId)
            .Include(wm => wm.Workspace)
            .Select(wm => new WorkspaceDto(
                wm.Workspace.Id,
                wm.Workspace.Name,
                wm.Workspace.Description,
                wm.Workspace.OwnerId,
                wm.Workspace.CreatedAt,
                wm.Workspace.UpdatedAt))
            .ToListAsync();
    }

    public async Task<WorkspaceDto> GetWorkspaceAsync(Guid id, Guid userId)
    {
        var workspace = await _db.Workspaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Workspace introuvable.");
        await EnsureMemberAsync(id, userId);
        return ToDto(workspace);
    }

    public async Task<WorkspaceDto> CreateWorkspaceAsync(CreateWorkspaceRequest request, Guid userId)
    {
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Workspaces.Add(workspace);
        _db.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = userId,
            Role = WorkspaceRole.Owner,
            JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return ToDto(workspace);
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(Guid id, UpdateWorkspaceRequest request, Guid userId)
    {
        var workspace = await _db.Workspaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Workspace introuvable.");
        await EnsureRoleAsync(id, userId, WorkspaceRole.Owner, WorkspaceRole.Admin);
        workspace.Name = request.Name;
        workspace.Description = request.Description;
        workspace.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(workspace);
    }

    public async Task DeleteWorkspaceAsync(Guid id, Guid userId)
    {
        var workspace = await _db.Workspaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Workspace introuvable.");
        await EnsureRoleAsync(id, userId, WorkspaceRole.Owner);
        _db.Workspaces.Remove(workspace);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureMemberAsync(Guid workspaceId, Guid userId)
    {
        if (!await _db.WorkspaceMembers.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");
    }

    private async Task EnsureRoleAsync(Guid workspaceId, Guid userId, params WorkspaceRole[] roles)
    {
        var member = await _db.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
        if (member == null || !roles.Contains(member.Role))
            throw new UnauthorizedAccessException("Permissions insuffisantes.");
    }

    private static WorkspaceDto ToDto(Workspace w) =>
        new(w.Id, w.Name, w.Description, w.OwnerId, w.CreatedAt, w.UpdatedAt);
}
