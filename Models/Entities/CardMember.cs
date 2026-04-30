namespace velcro.Models.Entities;

public class CardMember
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }

    public Card Card { get; set; } = null!;
    public User User { get; set; } = null!;
}
