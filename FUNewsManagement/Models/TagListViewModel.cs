using BusinessObjects.Models;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class TagListViewModel
    {
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public string? SearchKeyword { get; set; }
    }
}