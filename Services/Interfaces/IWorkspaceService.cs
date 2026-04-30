using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId);
    Task<WorkspaceDto> GetWorkspaceAsync(Guid id, Guid userId);
    Task<WorkspaceDto> CreateWorkspaceAsync(CreateWorkspaceRequest request, Guid userId);
    Task<WorkspaceDto> UpdateWorkspaceAsync(Guid id, UpdateWorkspaceRequest request, Guid userId);
    Task DeleteWorkspaceAsync(Guid id, Guid userId);
}
