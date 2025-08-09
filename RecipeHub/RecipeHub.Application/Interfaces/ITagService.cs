using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.Tag;

namespace RecipeHub.Application.Interfaces;

public interface ITagService
{
    // Métodos básicos
    Task<IEnumerable<TagListViewModel>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    Task<PaginationResult<TagListViewModel>> GetTagsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<TagDetailViewModel?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TagDetailViewModel?> GetTagByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Guid> AddTagAsync(TagCreateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> UpdateTagAsync(Guid id, TagUpdateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Métodos de contagem
    Task<int> GetTotalTagsCountAsync(CancellationToken cancellationToken = default);
}
