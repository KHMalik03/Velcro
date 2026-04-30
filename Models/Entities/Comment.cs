namespace velcro.Models.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Card Card { get; set; } = null!;
    public User Author { get; set; } = null!;
}
