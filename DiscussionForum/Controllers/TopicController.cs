
using DiscussionForum.Data;
using DiscussionForum.Models;
using DiscussionForum.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscussionForum.Controllers
{
    public class TopicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IdentityService _identityService;

        public TopicController(
            ApplicationDbContext context,
            IdentityService identityService)
        {
            _context = context;
            _identityService = identityService;
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
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to create a topic.");
            }

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
            topic.CreatedByUserId = currentUser.Id;
            topic.CreatedByName = currentUser.Name;

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

        // Display topic details and replies
        // Display topic details and replies
        public async Task<IActionResult> Details(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.DiscussionBoard)
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            ViewBag.CurrentUserId = currentUser?.Id;

            ViewBag.IsAdmin = string.Equals(
                currentUser?.Role,
                "admin",
                StringComparison.OrdinalIgnoreCase);

            return View(topic);
        }

        // Display the edit topic page
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to edit a topic.");
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && topic.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only edit your own topics.";

                return RedirectToAction(
                    "Details", new { id = topic.TopicId });
            }

            ViewBag.BoardTitle = await _context.DiscussionBoards
                .Where(b => b.BoardId == topic.BoardId)
                .Select(b => b.Title)
                .FirstOrDefaultAsync();

            return View(topic);
        }

        // Save the edited topic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Title,Description,BoardId")] Topic editedTopic)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to edit a topic.");
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && topic.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only edit your own topics.";

                return RedirectToAction(
                    "Details", new { id = topic.TopicId });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BoardTitle = await _context.DiscussionBoards
                    .Where(b => b.BoardId == topic.BoardId)
                    .Select(b => b.Title)
                    .FirstOrDefaultAsync();

                return View(editedTopic);
            }

            // Only update the editable fields.
            // Preserve original owner, date and board.
            topic.Title = editedTopic.Title.Trim();
            topic.Description = editedTopic.Description.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Topic updated successfully.";

            return RedirectToAction(
                "Details", new { id = topic.TopicId });
        }

        // Delete a topic and its associated replies
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to delete a topic.");
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && topic.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only delete your own topics.";

                return RedirectToAction(
                    "Details", new { id = topic.TopicId });
            }

            int boardId = topic.BoardId;

            // Remove all replies belonging to this topic.
            var replies = await _context.Replies
                .Where(r => r.TopicId == id)
                .ToListAsync();

            _context.Replies.RemoveRange(replies);

            // Remove the topic.
            _context.Topics.Remove(topic);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Topic deleted successfully.";

            return RedirectToAction(
                "ViewDiscussion", "DiscussionBoard",
                new { id = boardId });
        }

        // Post a reply or nested reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReply(
            int topicId,
            string content,
            int? parentReplyId)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to post a reply.");
            }

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
                    return BadRequest(
                        "The reply you selected was not found.");
                }
            }

            var reply = new Reply
            {
                TopicId = topicId,
                Content = content,
                CreatedDate = DateTime.UtcNow,
                ParentReplyId = parentReplyId,
                CreatedByUserId = currentUser.Id,
                CreatedByName = currentUser.Name
            };

            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = topicId });
        }

        // Display the edit reply page
        public async Task<IActionResult> EditReply(int id)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to edit a reply.");
            }

            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && reply.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only edit your own replies.";

                return RedirectToAction(
                    "Details", new { id = reply.TopicId });
            }

            return View(reply);
        }

        // Save the edited reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int id, string content)
        {
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to edit a reply.");
            }

            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && reply.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only edit your own replies.";

                return RedirectToAction(
                    "Details", new { id = reply.TopicId });
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
            var currentUser =
                await _identityService.GetCurrentUserAsync(HttpContext);

            if (currentUser == null || !currentUser.Active)
            {
                return Unauthorized("Please sign in to delete a reply.");
            }

            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
            {
                return NotFound("Reply not found.");
            }

            bool isAdmin = string.Equals(
                currentUser.Role, "admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && reply.CreatedByUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You can only delete your own replies.";

                return RedirectToAction(
                    "Details", new { id = reply.TopicId });
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

            return RedirectToAction(
                "Details", new { id = topicId });
        }
    }
}
