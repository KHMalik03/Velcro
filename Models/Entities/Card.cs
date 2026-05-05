namespace velcro.Models.Entities;

// Carte Kanban : élément de base, appartient à une liste, contient des commentaires
public class Card
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }     // index d'ordre dans la liste (0-based), géré manuellement
    public DateTime? DueDate { get; set; }
    public bool IsArchived { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List List { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];
}
