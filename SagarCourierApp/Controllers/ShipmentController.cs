using Microsoft.AspNetCore.Mvc;

namespace SagarCourierApp.Controllers
{
    public class ShipmentController : Controller
    {
        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        public IActionResult GetShipments()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult CreateShipment()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult UpdateShipment(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id <= 0)
            {
                return RedirectToAction("GetShipments");
            }

            return View();
        }
    }
}