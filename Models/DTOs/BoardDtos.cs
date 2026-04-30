namespace velcro.Models.DTOs;

public record BoardSummaryDto(Guid Id, Guid WorkspaceId, string Name, string? Description, string BackgroundColor, bool IsArchived);
public record BoardMemberDto(Guid UserId, string Username, string? AvatarUrl, string Role);
public record BoardDto(Guid Id, Guid WorkspaceId, string Name, string? Description, string BackgroundColor, bool IsArchived, List<BoardMemberDto> Members, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateBoardRequest(Guid WorkspaceId, string Name, string? Description, string? BackgroundColor);
public record UpdateBoardRequest(string? Name, string? Description, string? BackgroundColor, bool? IsArchived);
