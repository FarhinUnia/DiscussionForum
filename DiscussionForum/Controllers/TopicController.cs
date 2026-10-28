
using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiscussionForum.Controllers
{
    public class TopicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display the topic creation page
        public async Task<IActionResult> Create(int boardId)
        {
            var board = await _context.DiscussionBoards
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

            if (board == null)
            {
                return NotFound("Discussion board not found.");
            }

            ViewBag.BoardId = board.BoardId;
            ViewBag.BoardTitle = board.Title;

            return View();
        }

        // Save the new topic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Topic topic)
        {
            var board = await _context.DiscussionBoards
                .FirstOrDefaultAsync(b => b.BoardId == topic.BoardId);

            if (board == null)
            {
                ModelState.AddModelError(
                    "BoardId", "Please select a valid discussion board.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BoardId = topic.BoardId;
                ViewBag.BoardTitle = board?.Title;
                return View(topic);
            }

            topic.CreatedDate = DateTime.Now;

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return RedirectToAction(
    "ViewDiscussion", "DiscussionBoard",
    new { id = topic.BoardId });
        }

        // Display topics for a discussion board
        public async Task<IActionResult> Index(int boardId)
        {
            var board = await _context.DiscussionBoards
                .Include(b => b.Topics)
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

            if (board == null)
            {
                return NotFound("Discussion board not found.");
            }

            ViewBag.BoardTitle = board.Title;
            ViewBag.BoardId = board.BoardId;

            return View(board.Topics
                .OrderByDescending(t => t.CreatedDate)
                .ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.DiscussionBoard)
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReply(
            int topicId,
            string content,
            int? parentReplyId)
        {
            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == topicId);

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Reply cannot be empty.");
            }

            content = content.Trim();

            if (content.Length > 1000)
            {
                return BadRequest("Reply cannot exceed 1000 characters.");
            }

            if (parentReplyId.HasValue)
            {
                var parentReply = await _context.Replies
                    .FirstOrDefaultAsync(r =>
                        r.ReplyId == parentReplyId.Value &&
                        r.TopicId == topicId);

                if (parentReply == null)
                {
                    return BadRequest("The reply you selected was not found.");
                }
            }

            var reply = new Reply
            {
                TopicId = topicId,
                Content = content,
                CreatedDate = DateTime.Now,
                ParentReplyId = parentReplyId
            };

            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = topicId });
        }


        // Display the edit reply page
        public async Task<IActionResult> EditReply(int id)
        {
            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            return View(reply);
        }

        // Save the edited reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int id, string content)
        {
            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(
                    "content", "Reply cannot be empty.");

                return View(reply);
            }

            content = content.Trim();

            if (content.Length > 1000)
            {
                ModelState.AddModelError(
                    "content", "Reply cannot exceed 1000 characters.");

                return View(reply);
            }

            reply.Content = content;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", new { id = reply.TopicId });
        }


        // Delete a reply and all replies nested under it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            int topicId = reply.TopicId;

            // Find the reply and all its nested replies
            var replies = await _context.Replies
                .Where(r => r.TopicId == topicId)
                .ToListAsync();

            var repliesToDelete = new List<Reply> { reply };
            var parentIds = new HashSet<int> { reply.ReplyId };

            while (true)
            {
                var children = replies
                    .Where(r => r.ParentReplyId.HasValue
                        && parentIds.Contains(r.ParentReplyId.Value)
                        && !repliesToDelete.Any(d =>
                            d.ReplyId == r.ReplyId))
                    .ToList();

                if (children.Count == 0)
                {
                    break;
                }

                foreach (var child in children)
                {
                    repliesToDelete.Add(child);
                    parentIds.Add(child.ReplyId);
                }
            }

            _context.Replies.RemoveRange(repliesToDelete);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = topicId });
        }


    }
}

