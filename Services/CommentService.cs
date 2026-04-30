using Microsoft.EntityFrameworkCore;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;

namespace velcro.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;

    public CommentService(ApplicationDbContext db) => _db = db;

    public async Task<CommentDto> AddCommentAsync(Guid cardId, CreateCommentRequest request, Guid userId)
    {
        var card = await _db.Cards.FindAsync(cardId)
            ?? throw new KeyNotFoundException("Carte introuvable.");
        await EnsureBoardMemberViaListAsync(card.ListId, userId);

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            AuthorId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var author = await _db.Users.FindAsync(userId);
        return ToDto(comment, author!.Username);
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid cardId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _db.Comments.Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.CardId == cardId)
            ?? throw new KeyNotFoundException("Commentaire introuvable.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Seul l'auteur peut modifier ce commentaire.");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(comment, comment.Author.Username);
    }

    public async Task DeleteCommentAsync(Guid cardId, Guid commentId, Guid userId)
    {
        var comment = await _db.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.CardId == cardId)
            ?? throw new KeyNotFoundException("Commentaire introuvable.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Seul l'auteur peut supprimer ce commentaire.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureBoardMemberViaListAsync(Guid listId, Guid userId)
    {
        var list = await _db.Lists.FindAsync(listId)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        if (!await _db.BoardMembers.AnyAsync(bm => bm.BoardId == list.BoardId && bm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");
    }

    private static CommentDto ToDto(Comment c, string authorUsername) =>
        new(c.Id, c.CardId, c.AuthorId, authorUsername, c.Content, c.CreatedAt, c.UpdatedAt);
}
