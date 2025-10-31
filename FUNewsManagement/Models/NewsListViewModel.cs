using BusinessObjects.Models;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class NewsListViewModel
    {
        public List<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
        public string? SearchKeyword { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}