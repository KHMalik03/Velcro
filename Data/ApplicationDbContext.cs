using Microsoft.EntityFrameworkCore;
using velcro.Models.Entities;
using EntityList = velcro.Models.Entities.List;

namespace velcro.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardMember> BoardMembers { get; set; }
    public DbSet<EntityList> Lists { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<CardLabel> CardLabels { get; set; }
    public DbSet<CardMember> CardMembers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Checklist> Checklists { get; set; }
    public DbSet<ChecklistItem> ChecklistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => rt.Token).IsUnique();
            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
        });

        modelBuilder.Entity<Workspace>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkspaceMember>(e =>
        {
            e.HasKey(wm => new { wm.WorkspaceId, wm.UserId });
            e.HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId);
            e.HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMemberships)
                .HasForeignKey(wm => wm.UserId);
        });

        modelBuilder.Entity<Board>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.BackgroundColor).HasDefaultValue("#0079BF");
            e.HasOne(b => b.Workspace)
                .WithMany(w => w.Boards)
                .HasForeignKey(b => b.WorkspaceId);
        });

        modelBuilder.Entity<BoardMember>(e =>
        {
            e.HasKey(bm => new { bm.BoardId, bm.UserId });
            e.HasOne(bm => bm.Board)
                .WithMany(b => b.Members)
                .HasForeignKey(bm => bm.BoardId);
            e.HasOne(bm => bm.User)
                .WithMany(u => u.BoardMemberships)
                .HasForeignKey(bm => bm.UserId);
        });

        modelBuilder.Entity<EntityList>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Board)
                .WithMany(b => b.Lists)
                .HasForeignKey(l => l.BoardId);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.List)
                .WithMany(l => l.Cards)
                .HasForeignKey(c => c.ListId);
            e.HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Label>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Color).HasDefaultValue("#61BD4F");
            e.HasOne(l => l.Board)
                .WithMany(b => b.Labels)
                .HasForeignKey(l => l.BoardId);
        });

        modelBuilder.Entity<CardLabel>(e =>
        {
            e.HasKey(cl => new { cl.CardId, cl.LabelId });
            e.HasOne(cl => cl.Card)
                .WithMany(c => c.CardLabels)
                .HasForeignKey(cl => cl.CardId);
            e.HasOne(cl => cl.Label)
                .WithMany(l => l.CardLabels)
                .HasForeignKey(cl => cl.LabelId);
        });

        modelBuilder.Entity<CardMember>(e =>
        {
            e.HasKey(cm => new { cm.CardId, cm.UserId });
            e.HasOne(cm => cm.Card)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.CardId);
            e.HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Card)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.CardId);
            e.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Checklist>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Card)
                .WithMany(c => c.Checklists)
                .HasForeignKey(c => c.CardId);
        });

        modelBuilder.Entity<ChecklistItem>(e =>
        {
            e.HasKey(ci => ci.Id);
            e.HasOne(ci => ci.Checklist)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.ChecklistId);
        });
    }
}
