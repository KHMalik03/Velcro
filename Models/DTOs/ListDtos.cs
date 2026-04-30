namespace velcro.Models.DTOs;

public record ListDto(Guid Id, Guid BoardId, string Name, int Position, bool IsArchived, DateTime CreatedAt, DateTime UpdatedAt);
public record ListWithCardsDto(Guid Id, Guid BoardId, string Name, int Position, bool IsArchived, List<CardSummaryDto> Cards, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateListRequest(Guid BoardId, string Name);
public record UpdateListRequest(string? Name, int? Position, bool? IsArchived);
public record ListPositionDto(Guid Id, int Position);
public record ReorderListsRequest(List<ListPositionDto> Lists);
