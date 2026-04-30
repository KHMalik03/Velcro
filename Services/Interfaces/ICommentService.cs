using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(Guid cardId, CreateCommentRequest request, Guid userId);
    Task<CommentDto> UpdateCommentAsync(Guid cardId, Guid commentId, UpdateCommentRequest request, Guid userId);
    Task<Guid> DeleteCommentAsync(Guid cardId, Guid commentId, Guid userId);
}
