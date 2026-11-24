
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
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

        // Display discussion boards for the selected course
        public async Task<IActionResult> Index(int? courseId, int? boardId)
        {
            // If coming from a topic page, get the course from its discussion board.
            if (boardId.HasValue)
            {
                courseId = await _context.DiscussionBoards
                    .Where(b => b.BoardId == boardId.Value)
                    .Select(b => (int?)b.CourseId)
                    .FirstOrDefaultAsync();

                if (!courseId.HasValue)
                {
                    return NotFound("Discussion board not found.");
                }
            }

            if (!courseId.HasValue)
            {
                return RedirectToAction("Index", "Courses");
            }

            var boards = await _context.DiscussionBoards
                .Where(b => b.CourseId == courseId.Value)
                .ToListAsync();

            ViewBag.CourseId = courseId.Value;
            ViewBag.IsAdmin = await IsAdminAsync();

            return View(boards);
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
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = board.CourseId;
                return View(board);
            }

            board.CreatedDate = DateTime.Now;

            _context.DiscussionBoards.Add(board);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                new { courseId = board.CourseId });
        }

        // View a discussion board
        public async Task<IActionResult> ViewDiscussion(int id)
        {
            var board = await _context.DiscussionBoards
                .Include(b => b.Topics)
                .FirstOrDefaultAsync(b => b.BoardId == id);

            if (board == null)
            {
                return NotFound("Discussion board not found.");
            }

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
