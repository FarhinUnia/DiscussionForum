using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using DiscussionForum.Services;
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


        public IActionResult Index(int? courseId)
        {
            var boards = _context.DiscussionBoards.AsQueryable();

            if (courseId.HasValue)
            {
                boards = boards.Where(b => b.CourseId == courseId.Value);
                ViewBag.CourseId = courseId.Value;
            }

            return View(boards.ToList());
        }



        public IActionResult Create(int? courseId)
        {
            if (courseId.HasValue)
            {
                ViewBag.CourseId = courseId.Value;
            }

            return View();
        }


        [HttpPost]
        public IActionResult Create(DiscussionBoard board)
        {
            if (ModelState.IsValid)
            {
                board.CreatedDate = DateTime.Now;

                _context.DiscussionBoards.Add(board);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(board);
        }




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

            // Get the currently logged-in user
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            // Pass user information to the view
            ViewBag.CurrentUserId = currentUser?.Id;

            ViewBag.IsAdmin = string.Equals(
                currentUser?.Role,
                "admin",
                StringComparison.OrdinalIgnoreCase);

            return View(board);
        }
    }
}