namespace velcro.Models.DTOs;

public record WorkspaceDto(Guid Id, string Name, string? Description, Guid OwnerId, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateWorkspaceRequest(string Name, string? Description);
public record UpdateWorkspaceRequest(string Name, string? Description);
