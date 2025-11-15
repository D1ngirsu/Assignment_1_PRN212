using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FUNewsManagement.Hubs  // Thay bằng namespace phù hợp
{
    public class SignalrServer : Hub
    {
        // Có thể thêm methods nếu cần, nhưng ở lab cơ bản thì để trống
        public async Task SendReloadNews()
        {
            await Clients.All.SendAsync("LoadNews");
        }

        public async Task SendReloadCategories()
        {
            await Clients.All.SendAsync("LoadCategories");
        }

        public async Task SendReloadTags()
        {
            await Clients.All.SendAsync("LoadTags");
        }

        public async Task SendReloadAccounts()
        {
            await Clients.All.SendAsync("LoadAccounts");
        }
    }
}