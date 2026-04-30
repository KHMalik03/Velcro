namespace velcro.Models.Entities;

public class Checklist
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Card Card { get; set; } = null!;
    public ICollection<ChecklistItem> Items { get; set; } = [];
}
