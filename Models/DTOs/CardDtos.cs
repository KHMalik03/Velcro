namespace velcro.Models.DTOs;

public record CardSummaryDto(Guid Id, Guid ListId, string Title, int Position, DateTime? DueDate, bool IsArchived);
public record CardDetailDto(Guid Id, Guid ListId, Guid BoardId, string Title, string? Description, int Position, DateTime? DueDate, bool IsArchived, Guid CreatedById, List<CommentDto> Comments, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateCardRequest(Guid ListId, string Title, string? Description, DateTime? DueDate);
public record UpdateCardRequest(string? Title, string? Description, DateTime? DueDate, bool? IsArchived);
public record MoveCardRequest(Guid TargetListId, int NewPosition);
