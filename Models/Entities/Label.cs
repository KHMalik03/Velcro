namespace velcro.Models.Entities;

public class Label
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#61BD4F";

    public Board Board { get; set; } = null!;
    public ICollection<CardLabel> CardLabels { get; set; } = [];
}
