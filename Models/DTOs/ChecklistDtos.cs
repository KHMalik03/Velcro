namespace velcro.Models.DTOs;

public record ChecklistItemDto(Guid Id, Guid ChecklistId, string Title, bool IsCompleted, int Position);
public record ChecklistDto(Guid Id, Guid CardId, string Title, List<ChecklistItemDto> Items);
