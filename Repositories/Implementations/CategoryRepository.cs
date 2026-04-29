using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.Include(c => c.Documents).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetForEditAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<bool> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Category category)
        {
            bool isUsedByDocuments = await _context.Documents.AnyAsync(d => d.CategoryId == category.Id);

            if (isUsedByDocuments)
            {
                // Trả về false để Controller biết mà hiện thông báo lỗi: 
                // "Không thể xóa danh mục đang có dữ liệu!"
                return false; 
            }

            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}