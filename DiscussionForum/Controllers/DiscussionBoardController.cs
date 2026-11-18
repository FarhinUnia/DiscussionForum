
using DiscussionForum.Data;
using DiscussionForum.Models;
using DiscussionForum.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscussionForum.Controllers
{
    public class DiscussionBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IdentityService _identityService;

        public DiscussionBoardController(
            ApplicationDbContext context,
            IdentityService identityService)
        {
            _context = context;
            _identityService = identityService;
        }

        // Check whether the current user is an admin
        private async Task<bool> IsAdminAsync()
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            return string.Equals(
                currentUser?.Role,
                "admin",
                StringComparison.OrdinalIgnoreCase);
        }

        // Display discussion boards
        public async Task<IActionResult> Index(int? courseId)
        {
            var boards = _context.DiscussionBoards.AsQueryable();

            if (courseId.HasValue)
            {
                boards = boards.Where(b => b.CourseId == courseId.Value);
                ViewBag.CourseId = courseId.Value;
            }

            ViewBag.IsAdmin = await IsAdminAsync();

            return View(await boards.ToListAsync());
        }

        // Display Create Board page (Admin only)
        public async Task<IActionResult> Create(int? courseId)
        {
            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] =
                    "Only administrators can create discussion boards.";

                return RedirectToAction("Index", new { courseId });
            }

            if (courseId.HasValue)
            {
                ViewBag.CourseId = courseId.Value;
            }

            return View();
        }

        // Save new discussion board (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscussionBoard board)
        {
            if (!await IsAdminAsync())
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (ModelState.IsValid)
            {
                board.CreatedDate = DateTime.Now;

                _context.DiscussionBoards.Add(board);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(board);
        }

        // View a discussion board (All logged-in users)
        public async Task<IActionResult> ViewDiscussion(int id)
        {
            var board = await _context.DiscussionBoards
                .FirstOrDefaultAsync(b => b.BoardId == id);

            if (board == null)
            {
                return NotFound();
            }

            board.Topics = await _context.Topics
                .Where(t => t.BoardId == id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            ViewBag.CurrentUserId = currentUser?.Id;

            ViewBag.IsAdmin = string.Equals(
                currentUser?.Role,
                "admin",
                StringComparison.OrdinalIgnoreCase);

            return View(board);
        }
    }
}
