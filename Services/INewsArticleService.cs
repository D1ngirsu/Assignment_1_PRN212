using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Services
{
    public interface INewsArticleService
    {
        Task<IEnumerable<NewsArticle>> GetAllNewsAsync();
        Task<NewsArticle?> GetNewsByIdAsync(string newsArticleId);
        Task<IEnumerable<NewsArticle>> GetActiveNewsAsync();
        Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(short categoryId);
        Task<IEnumerable<NewsArticle>> GetNewsByAuthorAsync(short authorId);
        Task<IEnumerable<NewsArticle>> GetNewsByTagAsync(int tagId);
        Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword);
        Task<IEnumerable<NewsArticle>> GetLatestNewsAsync(int count);
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<NewsArticle> CreateNewsAsync(NewsArticle newsArticle);
        Task UpdateNewsAsync(NewsArticle newsArticle);
        Task DeleteNewsAsync(string newsArticleId);
        Task PublishNewsAsync(string newsArticleId);
        Task UnpublishNewsAsync(string newsArticleId);
    }
}
