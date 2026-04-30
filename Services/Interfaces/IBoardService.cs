using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface IBoardService
{
    Task<List<BoardSummaryDto>> GetBoardsByWorkspaceAsync(Guid workspaceId, Guid userId);
    Task<BoardDto> GetBoardAsync(Guid id, Guid userId);
    Task<BoardDto> CreateBoardAsync(CreateBoardRequest request, Guid userId);
    Task<BoardDto> UpdateBoardAsync(Guid id, UpdateBoardRequest request, Guid userId);
    Task DeleteBoardAsync(Guid id, Guid userId);
}
