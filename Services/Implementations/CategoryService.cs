using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryUpdateRequest?> GetForEditAsync(int id)
        {
            var category = await _categoryRepository.GetForEditAsync(id);
            return category == null ? null : _mapper.Map<CategoryUpdateRequest>(category);
        }

        public async Task<bool> CreateAsync(CategoryCreateRequest request)
        {
            var category = _mapper.Map<Category>(request);
            return await _categoryRepository.CreateAsync(category);
        }

        public async Task<bool> UpdateAsync(CategoryUpdateRequest request)
        {
            var category = await _categoryRepository.GetForEditAsync(request.Id);
            if (category == null) return false;

            // Đổ dữ liệu từ request vào record cũ trong DB
            _mapper.Map(request, category); 
            return await _categoryRepository.UpdateAsync(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            return await _categoryRepository.DeleteAsync(category);
        }
    }
}