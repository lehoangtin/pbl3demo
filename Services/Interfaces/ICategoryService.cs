using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;

namespace StudyShare.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse?> GetByIdAsync(int id);
        Task<CategoryUpdateRequest?> GetForEditAsync(int id);
        Task<bool> CreateAsync(CategoryCreateRequest request);
        Task<bool> UpdateAsync(CategoryUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<CategoryUpdateRequest?> GetForUpdateAsync(int id);
    }
}