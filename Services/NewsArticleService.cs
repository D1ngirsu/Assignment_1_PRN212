using BusinessObjects.Models;
using Repositories;

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
            // Business validation
            if (string.IsNullOrWhiteSpace(newsArticle.NewsArticleId))
                throw new ArgumentException("News Article ID is required");

            if (string.IsNullOrWhiteSpace(newsArticle.Headline))
                throw new ArgumentException("Headline is required");

            // Check if ID already exists
            var existing = await _newsArticleRepository.GetByIdAsync(newsArticle.NewsArticleId);
            if (existing != null)
                throw new InvalidOperationException($"News article with ID {newsArticle.NewsArticleId} already exists");

            // Set default values
            newsArticle.CreatedDate = newsArticle.CreatedDate ?? DateTime.Now;
            newsArticle.NewsStatus = newsArticle.NewsStatus ?? false;

            return await _newsArticleRepository.AddAsync(newsArticle);
        }

        public async Task UpdateNewsAsync(NewsArticle newsArticle)
        {
            // Business validation
            var existing = await _newsArticleRepository.GetByIdAsync(newsArticle.NewsArticleId);
            if (existing == null)
                throw new KeyNotFoundException($"News article with ID {newsArticle.NewsArticleId} not found");

            if (string.IsNullOrWhiteSpace(newsArticle.Headline))
                throw new ArgumentException("Headline is required");

            // Update modified date
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
    }
}