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
    public class TagDAO : GenericRepository<Tag>, ITagRepository
    {
        public TagDAO(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<Tag?> GetByNameAsync(string tagName)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.TagName.ToLower() == tagName.ToLower());
        }

        public async Task<IEnumerable<Tag>> GetTagsWithNewsArticlesAsync()
        {
            return await _context.Tags
                .Include(t => t.NewsArticles)
                .Where(t => t.NewsArticles.Any())
                .OrderByDescending(t => t.NewsArticles.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count)
        {
            return await _context.Tags
                .Include(t => t.NewsArticles)
                .OrderByDescending(t => t.NewsArticles.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> TagNameExistsAsync(string tagName)
        {
            return await _dbSet.AnyAsync(t => t.TagName.ToLower() == tagName.ToLower());
        }

        public async Task<Tag?> GetTagWithNewsArticlesAsync(int tagId)
        {
            return await _dbSet
                .Include(t => t.NewsArticles)
                .FirstOrDefaultAsync(t => t.TagId == tagId);
        }
    }
}