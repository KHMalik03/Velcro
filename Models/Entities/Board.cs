namespace velcro.Models.Entities;

public class Board
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BackgroundColor { get; set; } = "#0079BF";
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public ICollection<BoardMember> Members { get; set; } = [];
    public ICollection<List> Lists { get; set; } = [];
}
