
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface INewsArticleRepository
    {
        Task<IEnumerable<NewsArticle>> GetAllAsync();
        Task<NewsArticle?> GetByIdAsync(string newsArticleId);
        Task<NewsArticle?> GetNewsWithDetailsAsync(string newsArticleId);
        Task<IEnumerable<NewsArticle>> GetActiveNewsAsync();
        Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(short categoryId);
        Task<IEnumerable<NewsArticle>> GetNewsByAuthorAsync(short authorId);
        Task<IEnumerable<NewsArticle>> GetNewsByTagAsync(int tagId);
        Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword);
        Task<IEnumerable<NewsArticle>> GetLatestNewsAsync(int count);
        Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<NewsArticle> AddAsync(NewsArticle newsArticle);
        Task UpdateAsync(NewsArticle newsArticle);
        Task DeleteAsync(string newsArticleId);

        // Methods for tag handling (no explicit junction entity needed)
        Task AddTagsToNewsAsync(string newsArticleId, IEnumerable<int> tagIds);
        Task RemoveAllTagsFromNewsAsync(string newsArticleId);
    }
}