using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Repositories
{
    public interface INewsArticleRepository : IGenericRepository<NewsArticle>
    {
        Task<IEnumerable<NewsArticle>> GetActiveNewsAsync();
        Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(short categoryId);
        Task<IEnumerable<NewsArticle>> GetNewsByAuthorAsync(short authorId);
        Task<IEnumerable<NewsArticle>> GetNewsByTagAsync(int tagId);
        Task<NewsArticle?> GetNewsWithDetailsAsync(string newsArticleId);
        Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword);
        Task<IEnumerable<NewsArticle>> GetLatestNewsAsync(int count);
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
