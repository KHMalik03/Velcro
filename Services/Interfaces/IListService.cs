using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface IListService
{
    Task<ListDto> GetListAsync(Guid id, Guid userId);
    Task<ListDto> CreateListAsync(CreateListRequest request, Guid userId);
    Task<ListDto> UpdateListAsync(Guid id, UpdateListRequest request, Guid userId);
    Task DeleteListAsync(Guid id, Guid userId);
    Task ReorderListsAsync(Guid boardId, ReorderListsRequest request, Guid userId);
}
