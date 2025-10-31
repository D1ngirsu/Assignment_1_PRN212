using BusinessObjects.Models;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public NewsArticleService(INewsArticleRepository newsArticleRepository)
        {
            _newsArticleRepository = newsArticleRepository;
        }

        public async Task<IEnumerable<NewsArticle>> GetAllNewsAsync()
        {
            return await _newsArticleRepository.GetAllAsync();
        }

        public async Task<NewsArticle?> GetNewsByIdAsync(string newsArticleId)
        {
            return await _newsArticleRepository.GetNewsWithDetailsAsync(newsArticleId);
        }

        public async Task<IEnumerable<NewsArticle>> GetActiveNewsAsync()
        {
            return await _newsArticleRepository.GetActiveNewsAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(short categoryId)
        {
            return await _newsArticleRepository.GetNewsByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByAuthorAsync(short authorId)
        {
            return await _newsArticleRepository.GetNewsByAuthorAsync(authorId);
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByTagAsync(int tagId)
        {
            return await _newsArticleRepository.GetNewsByTagAsync(tagId);
        }

        public async Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<NewsArticle>();

            return await _newsArticleRepository.SearchNewsAsync(keyword);
        }

        public async Task<IEnumerable<NewsArticle>> GetLatestNewsAsync(int count)
        {
            if (count <= 0)
                count = 10;

            return await _newsArticleRepository.GetLatestNewsAsync(count);
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            return await _newsArticleRepository.GetNewsByDateRangeAsync(startDate, endDate);
        }

        public async Task<NewsArticle> CreateNewsAsync(NewsArticle newsArticle)
        {
            if (string.IsNullOrWhiteSpace(newsArticle.NewsArticleId))
                throw new ArgumentException("News Article ID is required");

            if (string.IsNullOrWhiteSpace(newsArticle.Headline))
                throw new ArgumentException("Headline is required");

            var existing = await _newsArticleRepository.GetByIdAsync(newsArticle.NewsArticleId);
            if (existing != null)
                throw new InvalidOperationException($"News article with ID {newsArticle.NewsArticleId} already exists");

            newsArticle.CreatedDate = newsArticle.CreatedDate ?? DateTime.Now;
            newsArticle.NewsStatus = newsArticle.NewsStatus ?? false;

            return await _newsArticleRepository.AddAsync(newsArticle);
        }

        public async Task UpdateNewsAsync(NewsArticle newsArticle)
        {
            var existing = await _newsArticleRepository.GetByIdAsync(newsArticle.NewsArticleId);
            if (existing == null)
                throw new KeyNotFoundException($"News article with ID {newsArticle.NewsArticleId} not found");

            if (string.IsNullOrWhiteSpace(newsArticle.Headline))
                throw new ArgumentException("Headline is required");

            newsArticle.ModifiedDate = DateTime.Now;

            await _newsArticleRepository.UpdateAsync(newsArticle);
        }

        public async Task DeleteNewsAsync(string newsArticleId)
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException($"News article with ID {newsArticleId} not found");

            await _newsArticleRepository.DeleteAsync(newsArticleId);
        }

        public async Task PublishNewsAsync(string newsArticleId)
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException($"News article with ID {newsArticleId} not found");

            newsArticle.NewsStatus = true;
            newsArticle.ModifiedDate = DateTime.Now;

            await _newsArticleRepository.UpdateAsync(newsArticle);
        }

        public async Task UnpublishNewsAsync(string newsArticleId)
        {
            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException($"News article with ID {newsArticleId} not found");

            newsArticle.NewsStatus = false;
            newsArticle.ModifiedDate = DateTime.Now;

            await _newsArticleRepository.UpdateAsync(newsArticle);
        }

        public async Task<int> GetNextNewsIdAsync()
        {
            var allNews = await GetAllNewsAsync();
            var maxId = allNews
                .Where(n => int.TryParse(n.NewsArticleId, out _))
                .Max(n => int.TryParse(n.NewsArticleId, out int id) ? id : 0);

            return maxId + 1;
        }

        // FIX: Sửa lỗi vòng lặp thừa
        public async Task AddTagsToNewsAsync(string newsArticleId, IEnumerable<int> tagIds)
        {
            if (string.IsNullOrWhiteSpace(newsArticleId) || tagIds == null || !tagIds.Any())
                return;

            var newsArticle = await _newsArticleRepository.GetByIdAsync(newsArticleId);
            if (newsArticle == null)
                throw new KeyNotFoundException($"News article with ID {newsArticleId} not found");

            // Gọi repository 1 lần duy nhất với toàn bộ tagIds
            await _newsArticleRepository.AddTagsToNewsAsync(newsArticleId, tagIds.Distinct());
        }

        public async Task RemoveAllTagsFromNewsAsync(string newsArticleId)
        {
            if (string.IsNullOrWhiteSpace(newsArticleId))
                return;

            await _newsArticleRepository.RemoveAllTagsFromNewsAsync(newsArticleId);
        }
    }
}