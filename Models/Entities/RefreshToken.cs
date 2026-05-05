namespace velcro.Models.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;      // propriété calculée : pas stockée en base
    public bool IsActive  => !IsRevoked && !IsExpired;          // valide seulement si non révoqué ET non expiré

    public User User { get; set; } = null!;
}
