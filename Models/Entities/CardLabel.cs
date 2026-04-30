namespace velcro.Models.Entities;

public class CardLabel
{
    public Guid CardId { get; set; }
    public Guid LabelId { get; set; }

    public Card Card { get; set; } = null!;
    public Label Label { get; set; } = null!;
}
