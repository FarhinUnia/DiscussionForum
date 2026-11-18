
using DiscussionForum.Models;
using DiscussionForum.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DiscussionForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly IdentityService _identityService;

        public HomeController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Courses");
        }

        // Temporary authentication test
        public async Task<IActionResult> IdentityTest()
        {
            var user = await _identityService.GetCurrentUserAsync(HttpContext);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "You are not logged in."
                });
            }

            return Ok(new
            {
                message = "Authentication successful!",
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Active
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
