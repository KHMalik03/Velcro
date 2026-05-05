using Microsoft.EntityFrameworkCore;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;

namespace velcro.Services;

public class CardService : ICardService
{
    private readonly ApplicationDbContext _db;

    public CardService(ApplicationDbContext db) => _db = db;

    public async Task<CardDetailDto> GetCardAsync(Guid id, Guid userId)
    {
        var card = await LoadCardAsync(id);
        await EnsureBoardMemberViaListAsync(card.ListId, userId); // contrôle d'accès
        return ToDetailDto(card);
    }

    public async Task<CardDetailDto> CreateCardAsync(CreateCardRequest request, Guid userId)
    {
        var list = await _db.Lists.FindAsync(request.ListId)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        await EnsureBoardMemberAsync(list.BoardId, userId);

        // La position = nombre de cartes existantes → la nouvelle carte s'ajoute en dernier
        var position = await _db.Cards.CountAsync(c => c.ListId == request.ListId && !c.IsArchived);
        var card = new Card
        {
            Id          = Guid.NewGuid(),
            ListId      = request.ListId,
            Title       = request.Title,
            Description = request.Description,
            DueDate     = request.DueDate,
            Position    = position,
            CreatedById = userId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };
        _db.Cards.Add(card);
        await _db.SaveChangesAsync();
        // On recharge avec LoadCardAsync pour inclure les navigations (List, Comments)
        return ToDetailDto(await LoadCardAsync(card.Id));
    }

    public async Task<CardDetailDto> UpdateCardAsync(Guid id, UpdateCardRequest request, Guid userId)
    {
        var card = await LoadCardAsync(id);
        await EnsureBoardMemberViaListAsync(card.ListId, userId);

        // Mise à jour partielle : on ne modifie que les champs fournis (non null)
        if (request.Title       != null)  card.Title       = request.Title;
        if (request.Description != null)  card.Description = request.Description;
        if (request.DueDate.HasValue)     card.DueDate     = request.DueDate;
        if (request.IsArchived.HasValue)  card.IsArchived  = request.IsArchived.Value;
        card.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDetailDto(card);
    }

    public async Task<Guid> DeleteCardAsync(Guid id, Guid userId)
    {
        var card = await _db.Cards.Include(c => c.List).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Carte introuvable.");
        await EnsureBoardMemberViaListAsync(card.ListId, userId);
        var boardId = card.List.BoardId; // récupéré avant suppression pour le broadcast SignalR
        _db.Cards.Remove(card);
        await _db.SaveChangesAsync();
        return boardId;
    }

    public async Task<CardDetailDto> MoveCardAsync(Guid id, MoveCardRequest request, Guid userId)
    {
        var card = await _db.Cards.FindAsync(id)
            ?? throw new KeyNotFoundException("Carte introuvable.");
        await EnsureBoardMemberViaListAsync(card.ListId, userId);

        // Combler le trou dans l'ancienne liste : décale les cartes qui suivaient
        var oldListCards = await _db.Cards
            .Where(c => c.ListId == card.ListId && c.Id != id && c.Position > card.Position)
            .ToListAsync();
        foreach (var c in oldListCards) c.Position--;

        // Faire de la place dans la nouvelle liste : décale les cartes à partir de la cible
        var newListCards = await _db.Cards
            .Where(c => c.ListId == request.TargetListId && c.Position >= request.NewPosition)
            .ToListAsync();
        foreach (var c in newListCards) c.Position++;

        card.ListId    = request.TargetListId;
        card.Position  = request.NewPosition;
        card.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDetailDto(await LoadCardAsync(id));
    }

    // Charge une carte avec toutes ses relations nécessaires à l'affichage
    private async Task<Card> LoadCardAsync(Guid id) =>
        await _db.Cards
            .Include(c => c.List)                            // pour obtenir le BoardId
            .Include(c => c.Comments).ThenInclude(co => co.Author) // commentaires avec auteur
            .FirstOrDefaultAsync(c => c.Id == id)
        ?? throw new KeyNotFoundException("Carte introuvable.");

    // Remonte List → Board pour vérifier l'appartenance au board
    private async Task EnsureBoardMemberViaListAsync(Guid listId, Guid userId)
    {
        var list = await _db.Lists.FindAsync(listId)
            ?? throw new KeyNotFoundException("Liste introuvable.");
        await EnsureBoardMemberAsync(list.BoardId, userId);
    }

    // Contrôle d'accès principal : seuls les membres du board peuvent agir sur ses cartes
    private async Task EnsureBoardMemberAsync(Guid boardId, Guid userId)
    {
        if (!await _db.BoardMembers.AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId))
            throw new UnauthorizedAccessException("Accès refusé.");
    }

    // Convertit l'entité en DTO renvoyé au client (on ne renvoie jamais l'entité directement)
    private static CardDetailDto ToDetailDto(Card c) => new(
        c.Id, c.ListId, c.List.BoardId, c.Title, c.Description, c.Position, c.DueDate, c.IsArchived, c.CreatedById,
        c.Comments.OrderBy(co => co.CreatedAt)
            .Select(co => new CommentDto(co.Id, co.CardId, co.AuthorId, co.Author.Username, co.Content, co.CreatedAt, co.UpdatedAt, c.List.BoardId))
            .ToList(),
        c.CreatedAt, c.UpdatedAt);
}
