using BusinessObjects.Models;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class AccountListViewModel
    {
        public List<SystemAccount> Accounts { get; set; } = new List<SystemAccount>();
        public string? SearchKeyword { get; set; }
    }
}