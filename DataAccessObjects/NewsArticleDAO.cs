
using BusinessObjects.Models;
using DataAccessObject;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class NewsArticleDAO : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        public NewsArticleDAO(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<NewsArticle?> GetByIdAsync(string newsArticleId)
        {
            return await _dbSet.FirstOrDefaultAsync(n => n.NewsArticleId == newsArticleId);
        }

        public async Task<IEnumerable<NewsArticle>> GetActiveNewsAsync()
        {
            return await _dbSet
                .Where(n => n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(short categoryId)
        {
            return await _dbSet
                .Where(n => n.CategoryId == categoryId && n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByAuthorAsync(short authorId)
        {
            return await _dbSet
                .Where(n => n.CreatedById == authorId && n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByTagAsync(int tagId)
        {
            return await _context.NewsArticles
                .Where(n => n.Tags.Any(t => t.TagId == tagId) && n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetNewsWithDetailsAsync(string newsArticleId)
        {
            return await _dbSet
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == newsArticleId);
        }

        public async Task<IEnumerable<NewsArticle>> SearchNewsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<NewsArticle>();

            return await _dbSet
                .Where(n => n.NewsStatus == true &&
                            (n.NewsTitle.Contains(keyword) ||
                             n.Headline.Contains(keyword) ||
                             n.NewsContent.Contains(keyword)))
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetLatestNewsAsync(int count)
        {
            if (count <= 0)
                count = 10;

            return await _dbSet
                .Where(n => n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            return await _dbSet
                .Where(n => n.NewsStatus == true &&
                            n.CreatedDate >= startDate &&
                            n.CreatedDate <= endDate)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<NewsArticle> AddAsync(NewsArticle newsArticle)
        {
            _dbSet.Add(newsArticle);
            await _context.SaveChangesAsync();
            return newsArticle;
        }

        public async Task UpdateAsync(NewsArticle newsArticle)
        {
            _dbSet.Update(newsArticle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string newsArticleId)
        {
            var entity = await GetByIdAsync(newsArticleId);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // New methods: Handle tags via navigation collections (EF manages "NewsTag" junction table automatically)
        public async Task AddTagsToNewsAsync(string newsArticleId, IEnumerable<int> tagIds)
        {
            if (string.IsNullOrWhiteSpace(newsArticleId) || !tagIds?.Any() == true)
                return;

            // Load news article with existing tags
            var newsArticle = await _dbSet
                .Include(na => na.Tags)  // Load current Tags to check duplicates
                .FirstOrDefaultAsync(na => na.NewsArticleId == newsArticleId);

            if (newsArticle == null)
                throw new KeyNotFoundException($"News article with ID {newsArticleId} not found");

            var tagsToAdd = await _context.Tags
                .Where(t => tagIds.Contains(t.TagId))
                .ToListAsync();

            foreach (var tag in tagsToAdd)
            {
                if (!newsArticle.Tags.Any(existingTag => existingTag.TagId == tag.TagId))
                {
                    newsArticle.Tags.Add(tag);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllTagsFromNewsAsync(string newsArticleId)
        {
            if (string.IsNullOrWhiteSpace(newsArticleId))
                return;

            // Load news article with tags
            var newsArticle = await _dbSet
                .Include(na => na.Tags)
                .FirstOrDefaultAsync(na => na.NewsArticleId == newsArticleId);

            if (newsArticle == null)
                return;

            // Clear collection (EF will delete from "NewsTag" on SaveChanges)
            newsArticle.Tags.Clear();

            await _context.SaveChangesAsync();
        }
    }
}