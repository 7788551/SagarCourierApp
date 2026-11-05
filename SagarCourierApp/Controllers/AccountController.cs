using Microsoft.AspNetCore.Mvc;

namespace SagarCourierApp.Controllers
{
    public class AccountController : Controller
    {

        public IActionResult Register()
        {
            ViewBag.Message = "Register your staff ";
            ViewData["Title"] = "Register";
            return View();
        }
        public IActionResult Login()
        {
            ViewBag.Message = "Login to your Account";
            ViewData["Title"] = "Login";
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login","Account");
        }

    }
}
