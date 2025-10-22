using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Repositories
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string tagName);
        Task<IEnumerable<Tag>> GetTagsWithNewsArticlesAsync();
        Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count);
        Task<bool> TagNameExistsAsync(string tagName);
        Task<Tag?> GetTagWithNewsArticlesAsync(int tagId);
    }
}
