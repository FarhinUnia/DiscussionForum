
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

        // =====================================
        // DISCUSSION BOARDS
        // =====================================

        // GET: api/discussionboards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiscussionBoard>>> GetBoards()
        {
            var boards = await _context.DiscussionBoards.ToListAsync();

            return Ok(boards);
        }

        // GET: api/discussionboards/1
        [HttpGet("{boardId:int}")]
        public async Task<ActionResult<DiscussionBoard>> GetBoard(int boardId)
        {
            var board = await _context.DiscussionBoards
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

            if (board == null)
                return NotFound("Discussion board not found.");

            return Ok(board);
        }

        // POST: api/discussionboards
        [HttpPost]
        public async Task<ActionResult<DiscussionBoard>> CreateBoard(
            DiscussionBoard board)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            board.CreatedDate = DateTime.UtcNow;

            _context.DiscussionBoards.Add(board);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBoard),
                new { boardId = board.BoardId },
                board);
        }

        // =====================================
        // TOPICS
        // =====================================

        // GET: api/discussionboards/1/topics
        [HttpGet("{boardId:int}/topics")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopics(int boardId)
        {
            var boardExists = await _context.DiscussionBoards
                .AnyAsync(b => b.BoardId == boardId);

            if (!boardExists)
                return NotFound("Discussion board not found.");

            var topics = await _context.Topics
                .Where(t => t.BoardId == boardId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return Ok(topics);
        }

        // GET: api/discussionboards/topics/1
        [HttpGet("topics/{topicId:int}")]
        public async Task<ActionResult<Topic>> GetTopic(int topicId)
        {
            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == topicId);

            if (topic == null)
                return NotFound("Topic not found.");

            return Ok(topic);
        }

        // POST: api/discussionboards/1/topics
        [HttpPost("{boardId:int}/topics")]
        public async Task<ActionResult<Topic>> CreateTopic(
            int boardId, Topic topic)
        {
            var boardExists = await _context.DiscussionBoards
                .AnyAsync(b => b.BoardId == boardId);

            if (!boardExists)
                return NotFound("Discussion board not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            topic.BoardId = boardId;
            topic.CreatedDate = DateTime.UtcNow;

            _context.Topics.Add(topic);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTopic),
                new { topicId = topic.TopicId },
                topic);
        }

        // =====================================
        // REPLIES
        // =====================================

        // GET: api/discussionboards/topics/1/replies
        [HttpGet("topics/{topicId:int}/replies")]
        public async Task<ActionResult<IEnumerable<Reply>>> GetReplies(
            int topicId)
        {
            var topicExists = await _context.Topics
                .AnyAsync(t => t.TopicId == topicId);

            if (!topicExists)
                return NotFound("Topic not found.");

            var replies = await _context.Replies
                .Where(r => r.TopicId == topicId)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();

            return Ok(replies);
        }

        // GET: api/discussionboards/replies/1
        [HttpGet("replies/{id:int}")]
        public async Task<ActionResult<Reply>> GetReply(int id)
        {
            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
                return NotFound("Reply not found.");

            return Ok(reply);
        }

        // POST: api/discussionboards/topics/1/replies
        [HttpPost("topics/{topicId:int}/replies")]
        public async Task<ActionResult<Reply>> CreateReply(
            int topicId, Reply reply)
        {
            var topicExists = await _context.Topics
                .AnyAsync(t => t.TopicId == topicId);

            if (!topicExists)
                return NotFound("Topic not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(reply.Content))
                return BadRequest("Reply cannot be empty.");

            reply.Content = reply.Content.Trim();

            if (reply.Content.Length > 1000)
                return BadRequest(
                    "Reply cannot exceed 1000 characters.");

            // Validate parent reply for threaded discussions
            if (reply.ParentReplyId.HasValue)
            {
                var parentExists = await _context.Replies
                    .AnyAsync(r =>
                        r.ReplyId == reply.ParentReplyId.Value &&
                        r.TopicId == topicId);

                if (!parentExists)
                    return BadRequest(
                        "Parent reply not found in this topic.");
            }

            reply.TopicId = topicId;
            reply.CreatedDate = DateTime.UtcNow;

            _context.Replies.Add(reply);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReply),
                new { id = reply.ReplyId },
                reply);
        }

        // =====================================
        // EDIT REPLIES
        // =====================================

        // PUT: api/discussionboards/replies/1
        [HttpPut("replies/{id:int}")]
        public async Task<IActionResult> EditReply(
            int id, Reply updatedReply)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
                return NotFound("Reply not found.");

            if (string.IsNullOrWhiteSpace(updatedReply.Content))
                return BadRequest("Reply cannot be empty.");

            var content = updatedReply.Content.Trim();

            if (content.Length > 1000)
                return BadRequest(
                    "Reply cannot exceed 1000 characters.");

            reply.Content = content;

            await _context.SaveChangesAsync();

            return Ok(reply);
        }

        // =====================================
        // DELETE REPLIES
        // =====================================

        // DELETE: api/discussionboards/replies/1
        [HttpDelete("replies/{id:int}")]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.Replies
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
                return NotFound("Reply not found.");

            // Get all replies belonging to the same topic
            var replies = await _context.Replies
                .Where(r => r.TopicId == reply.TopicId)
                .ToListAsync();

            // Find the selected reply and all its child replies
            var repliesToDelete = new List<Reply> { reply };

            var parentIds = new HashSet<int>
            {
                reply.ReplyId
            };

            while (true)
            {
                var children = replies
                    .Where(r =>
                        r.ParentReplyId.HasValue &&
                        parentIds.Contains(r.ParentReplyId.Value) &&
                        !repliesToDelete.Any(d =>
                            d.ReplyId == r.ReplyId))
                    .ToList();

                if (children.Count == 0)
                    break;

                foreach (var child in children)
                {
                    repliesToDelete.Add(child);
                    parentIds.Add(child.ReplyId);
                }
            }

            _context.Replies.RemoveRange(repliesToDelete);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
