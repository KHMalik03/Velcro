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
        var card = await _db.Cards.Include(c => c.List).FirstOrDefaultAsync(c => c.Id == cardId)
            ?? throw new KeyNotFoundException("Carte introuvable.");
        var boardId = await EnsureBoardMemberViaListAsync(card.ListId, userId);

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
        return ToDto(comment, author!.Username, boardId);
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid cardId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _db.Comments
            .Include(c => c.Author)
            .Include(c => c.Card).ThenInclude(card => card.List)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.CardId == cardId)
            ?? throw new KeyNotFoundException("Commentaire introuvable.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Seul l'auteur peut modifier ce commentaire.");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(comment, comment.Author.Username, comment.Card.List.BoardId);
    }

    public async Task<Guid> DeleteCommentAsync(Guid cardId, Guid commentId, Guid userId)
    {
        var comment = await _db.Comments
            .Include(c => c.Card).ThenInclude(card => card.List)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.CardId == cardId)
            ?? throw new KeyNotFoundException("Commentaire introuvable.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Seul l'auteur peut supprimer ce commentaire.");

        var boardId = comment.Card.List.BoardId;
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return boardId;
    }

    private async Task<Guid> EnsureBoardMemberViaListAsync(Guid listId, Guid userId)
    {
        var list = await _db.Lists.FindAsync(listId)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        if (!await _db.BoardMembers.AnyAsync(bm => bm.BoardId == list.BoardId && bm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");
        return list.BoardId;
    }

    private static CommentDto ToDto(Comment c, string authorUsername, Guid boardId) =>
        new(c.Id, c.CardId, c.AuthorId, authorUsername, c.Content, c.CreatedAt, c.UpdatedAt, boardId);
}
