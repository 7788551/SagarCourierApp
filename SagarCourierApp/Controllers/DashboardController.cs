using Microsoft.AspNetCore.Mvc;

namespace SagarCourierApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }
        public IActionResult StaffDashboard()
        {
            return View();
        }
    }
}
