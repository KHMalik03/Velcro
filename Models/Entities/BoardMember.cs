namespace velcro.Models.Entities;

public enum BoardRole { Admin, Member, Observer }

public class BoardMember
{
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
    public BoardRole Role { get; set; }
    public DateTime JoinedAt { get; set; }

    public Board Board { get; set; } = null!;
    public User User { get; set; } = null!;
}
