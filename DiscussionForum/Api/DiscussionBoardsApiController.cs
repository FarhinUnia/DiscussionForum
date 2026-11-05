
using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscussionForum.Api
{
    [ApiController]
    [Route("api/discussionboards")]
    public class DiscussionBoardsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiscussionBoardsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiscussionBoard>>> GetBoards()
        {
            var boards = await _context.DiscussionBoards.ToListAsync();
            return Ok(boards);
        }

        [HttpPost]
        public async Task<ActionResult<DiscussionBoard>> CreateBoard(
            DiscussionBoard board)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            board.CreatedDate = DateTime.Now;

            _context.DiscussionBoards.Add(board);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBoards),
                new { id = board.BoardId },
                board);
        }
    }
}

