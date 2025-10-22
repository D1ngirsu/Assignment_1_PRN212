using BusinessObjects.Models;
using Repositories;
using Services;

namespace Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetByIdAsync(tagId);
        }

        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            return await _tagRepository.GetByNameAsync(tagName);
        }

        public async Task<IEnumerable<Tag>> GetTagsWithNewsAsync()
        {
            return await _tagRepository.GetTagsWithNewsArticlesAsync();
        }

        public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count)
        {
            if (count <= 0)
                count = 10;

            return await _tagRepository.GetMostUsedTagsAsync(count);
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            // Business validation
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (string.IsNullOrWhiteSpace(tag.TagName))
                throw new ArgumentException("Tag name is required");

            // Check if tag name already exists
            if (await _tagRepository.TagNameExistsAsync(tag.TagName))
                throw new InvalidOperationException($"Tag with name {tag.TagName} already exists");

            return await _tagRepository.AddAsync(tag);
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            // Business validation
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var existing = await _tagRepository.GetByIdAsync(tag.TagId);
            if (existing == null)
                throw new KeyNotFoundException($"Tag with ID {tag.TagId} not found");

            if (string.IsNullOrWhiteSpace(tag.TagName))
                throw new ArgumentException("Tag name is required");

            // Check if tag name changed and exists
            if (tag.TagName != existing.TagName && await _tagRepository.TagNameExistsAsync(tag.TagName))
                throw new InvalidOperationException($"Tag with name {tag.TagName} already exists");

            await _tagRepository.UpdateAsync(tag);
        }

        public async Task DeleteTagAsync(int tagId)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with ID {tagId} not found");

            // Check if tag is used in news articles
            var tagWithNews = await _tagRepository.GetTagWithNewsArticlesAsync(tagId);
            if (tagWithNews != null && tagWithNews.NewsArticles.Any())
                throw new InvalidOperationException($"Cannot delete tag {tagId} as it is associated with news articles");

            await _tagRepository.DeleteAsync(tagId);
        }

        public async Task<bool> TagNameExistsAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            return await _tagRepository.TagNameExistsAsync(tagName);
        }
    }
}