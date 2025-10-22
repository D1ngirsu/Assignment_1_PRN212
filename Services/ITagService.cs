using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int tagId);
        Task<Tag?> GetTagByNameAsync(string tagName);
        Task<IEnumerable<Tag>> GetTagsWithNewsAsync();
        Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count);
        Task<Tag> CreateTagAsync(Tag tag);
        Task UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(int tagId);
        Task<bool> TagNameExistsAsync(string tagName);
    }
}
