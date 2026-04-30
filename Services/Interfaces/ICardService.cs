using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface ICardService
{
    Task<CardDetailDto> GetCardAsync(Guid id, Guid userId);
    Task<CardDetailDto> CreateCardAsync(CreateCardRequest request, Guid userId);
    Task<CardDetailDto> UpdateCardAsync(Guid id, UpdateCardRequest request, Guid userId);
    Task<Guid> DeleteCardAsync(Guid id, Guid userId);
    Task<CardDetailDto> MoveCardAsync(Guid id, MoveCardRequest request, Guid userId);
}
