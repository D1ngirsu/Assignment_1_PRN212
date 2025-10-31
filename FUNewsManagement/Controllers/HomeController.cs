using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
