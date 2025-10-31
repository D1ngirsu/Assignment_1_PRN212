using BusinessObjects.Models;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class CategoryListViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public string? SearchKeyword { get; set; }
    }
}