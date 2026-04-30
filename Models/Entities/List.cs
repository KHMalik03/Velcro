namespace velcro.Models.Entities;

public class List
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Board Board { get; set; } = null!;
    public ICollection<Card> Cards { get; set; } = [];
}
