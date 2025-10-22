using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(short categoryId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentCategoryId);
        Task<Category?> GetCategoryWithNewsAsync(short categoryId);
        Task<Category> CreateCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(short categoryId);
        Task<bool> CanDeleteCategoryAsync(short categoryId);
    }
}
