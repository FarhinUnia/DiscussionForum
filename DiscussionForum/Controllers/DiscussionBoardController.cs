using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace DiscussionForum.Controllers
{
    public class DiscussionBoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscussionBoardController(ApplicationDbContext context)
        {
            _context = context;
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

            return View(board);
        }

    }
}