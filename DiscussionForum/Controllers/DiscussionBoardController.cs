using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DiscussionForum.Controllers
{
    public class DiscussionBoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscussionBoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var boards = _context.DiscussionBoards.ToList();

            return View(boards);
        }

        public IActionResult Create()
        {
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
    }
}