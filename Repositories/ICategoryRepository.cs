using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentCategoryId);
        Task<Category?> GetCategoryWithNewsArticlesAsync(short categoryId);
        Task<bool> HasNewsArticlesAsync(short categoryId);
    }
}
