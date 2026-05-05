using Microsoft.EntityFrameworkCore;
using velcro.Models.Entities;
using EntityList = velcro.Models.Entities.List; // alias car List entre en conflit avec List<T> du système

namespace velcro.Data;

// DbContext : point d'entrée EF Core — représente la session avec la base de données
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSet = représentation d'une table SQL sous forme de collection C# requêtable (LINQ)
    public DbSet<User>            Users            { get; set; }
    public DbSet<RefreshToken>    RefreshTokens    { get; set; }
    public DbSet<Workspace>       Workspaces       { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<Board>           Boards           { get; set; }
    public DbSet<BoardMember>     BoardMembers     { get; set; }
    public DbSet<EntityList>      Lists            { get; set; }
    public DbSet<Card>            Cards            { get; set; }
    public DbSet<Comment>         Comments         { get; set; }

    // OnModelCreating : configuration Fluent API — définit les contraintes qui ne peuvent pas
    // être exprimées par les seuls attributs (clés composites, index uniques, DeleteBehavior...)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();    // un seul compte par email
            e.HasIndex(u => u.Username).IsUnique(); // un seul compte par username
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => rt.Token).IsUnique(); // le token doit être unique pour être retrouvé rapidement
            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
        });

        modelBuilder.Entity<Workspace>(e =>
        {
            e.HasKey(w => w.Id);
            // Restrict : empêche la suppression en cascade du propriétaire → doit supprimer le workspace d'abord
            e.HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkspaceMember>(e =>
        {
            e.HasKey(wm => new { wm.WorkspaceId, wm.UserId }); // clé composite : un utilisateur ne peut être membre qu'une fois
            e.HasOne(wm => wm.Workspace).WithMany(w => w.Members).HasForeignKey(wm => wm.WorkspaceId);
            e.HasOne(wm => wm.User).WithMany(u => u.WorkspaceMemberships).HasForeignKey(wm => wm.UserId);
        });

        modelBuilder.Entity<Board>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.BackgroundColor).HasDefaultValue("#0079BF");
            e.HasOne(b => b.Workspace).WithMany(w => w.Boards).HasForeignKey(b => b.WorkspaceId);
        });

        modelBuilder.Entity<BoardMember>(e =>
        {
            e.HasKey(bm => new { bm.BoardId, bm.UserId }); // clé composite
            e.HasOne(bm => bm.Board).WithMany(b => b.Members).HasForeignKey(bm => bm.BoardId);
            e.HasOne(bm => bm.User).WithMany(u => u.BoardMemberships).HasForeignKey(bm => bm.UserId);
        });

        modelBuilder.Entity<EntityList>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Board).WithMany(b => b.Lists).HasForeignKey(l => l.BoardId);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.List).WithMany(l => l.Cards).HasForeignKey(c => c.ListId);
            // Restrict : on ne peut pas supprimer un utilisateur qui a créé des cartes
            e.HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Card).WithMany(c => c.Comments).HasForeignKey(c => c.CardId);
            // Restrict : on ne peut pas supprimer un utilisateur qui a écrit des commentaires
            e.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
