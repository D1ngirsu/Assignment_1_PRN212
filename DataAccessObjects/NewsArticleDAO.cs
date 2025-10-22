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
            return await _dbSet
                .Where(n => n.NewsStatus == true)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(n => n.NewsStatus == true &&
                            n.CreatedDate >= startDate &&
                            n.CreatedDate <= endDate)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }
    }
}