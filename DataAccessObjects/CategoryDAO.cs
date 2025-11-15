using BusinessObjects.Models;
using DataAccessObject;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace DataAccessObjects
{
    public class CategoryDAO : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryDAO(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == null && c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentCategoryId)
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithNewsArticlesAsync(short categoryId)
        {
            return await _dbSet
                .Include(c => c.NewsArticles)
                .Include(c => c.InverseParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<bool> HasNewsArticlesAsync(short categoryId)
        {
            return await _context.NewsArticles
                .AnyAsync(n => n.CategoryId == categoryId);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public override async Task<Category?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == (short)id);
        }

        // CRITICAL FIX: Override UpdateAsync to handle nullable bool properly
        public override async Task UpdateAsync(Category entity)
        {
            // Get the existing tracked entity
            var existingEntity = await _dbSet.FindAsync(entity.CategoryId);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Category with ID {entity.CategoryId} not found");
            }

            // Explicitly update each property to ensure nullable bool is handled correctly
            existingEntity.CategoryName = entity.CategoryName;
            existingEntity.CategoryDesciption = entity.CategoryDesciption;
            existingEntity.ParentCategoryId = entity.ParentCategoryId;

            // CRITICAL: Explicitly set IsActive (nullable bool needs special handling)
            existingEntity.IsActive = entity.IsActive;

            // Mark as modified and save
            _context.Entry(existingEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}