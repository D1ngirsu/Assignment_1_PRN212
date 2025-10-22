using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(short categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository.GetActiveCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _categoryRepository.GetRootCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentCategoryId)
        {
            return await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
        }

        public async Task<Category?> GetCategoryWithNewsAsync(short categoryId)
        {
            return await _categoryRepository.GetCategoryWithNewsArticlesAsync(categoryId);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // Business validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("Category name is required");

            if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
                throw new ArgumentException("Category description is required");

            // Set default values
            category.IsActive = category.IsActive ?? true;

            return await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            // Business validation
            var existingCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Category with ID {category.CategoryId} not found");

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("Category name is required");

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(short categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            // Check if category has news articles
            if (await _categoryRepository.HasNewsArticlesAsync(categoryId))
                throw new InvalidOperationException("Cannot delete category that has news articles");

            await _categoryRepository.DeleteAsync(categoryId);
        }

        public async Task<bool> CanDeleteCategoryAsync(short categoryId)
        {
            return !await _categoryRepository.HasNewsArticlesAsync(categoryId);
        }
    }
}