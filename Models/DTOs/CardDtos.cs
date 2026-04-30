namespace velcro.Models.DTOs;

public record CardMemberDto(Guid UserId, string Username, string? AvatarUrl);
public record CardSummaryDto(Guid Id, Guid ListId, string Title, int Position, DateTime? DueDate, bool IsArchived, List<CardMemberDto> Members, List<LabelDto> Labels);
public record CardDetailDto(Guid Id, Guid ListId, Guid BoardId, string Title, string? Description, int Position, DateTime? DueDate, bool IsArchived, Guid CreatedById, List<CardMemberDto> Members, List<LabelDto> Labels, List<CommentDto> Comments, List<ChecklistDto> Checklists, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateCardRequest(Guid ListId, string Title, string? Description, DateTime? DueDate);
public record UpdateCardRequest(string? Title, string? Description, DateTime? DueDate, bool? IsArchived);
public record MoveCardRequest(Guid TargetListId, int NewPosition);
