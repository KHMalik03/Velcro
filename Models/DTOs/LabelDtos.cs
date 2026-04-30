namespace velcro.Models.DTOs;

public record LabelDto(Guid Id, Guid BoardId, string Name, string Color);
public record CreateLabelRequest(Guid BoardId, string Name, string Color);
