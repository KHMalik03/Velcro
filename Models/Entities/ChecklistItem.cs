namespace velcro.Models.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Position { get; set; }

    public Checklist Checklist { get; set; } = null!;
}
