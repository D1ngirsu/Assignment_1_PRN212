using BusinessObjects.Models;
using System;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class NewsReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1).Date;
        public DateTime EndDate { get; set; } = DateTime.Now.Date;
        public List<NewsArticle> NewsInPeriod { get; set; } = new List<NewsArticle>();
        public int TotalNews { get; set; }
        public string? SortBy { get; set; } = "CreatedDate"; // Descending by default
    }
}