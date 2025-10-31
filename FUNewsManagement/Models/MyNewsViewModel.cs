using BusinessObjects.Models;
using System.Collections.Generic;

namespace FUNewsManagement.Models.ViewModels
{
    public class MyNewsViewModel : NewsListViewModel
    {
        public short UserId { get; set; }
    }
}