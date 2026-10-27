
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }

        [Required(ErrorMessage = "Reply cannot be empty.")]
        [StringLength(1000, MinimumLength = 1,
            ErrorMessage = "Reply must be between 1 and 1000 characters.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int TopicId { get; set; }

        public Topic? Topic { get; set; }

        // Null means this is a direct reply to the topic.
        // A value means this reply belongs to another reply.
        public int? ParentReplyId { get; set; }

        public Reply? ParentReply { get; set; }

        public ICollection<Reply> ChildReplies { get; set; }
            = new List<Reply>();
    }
}
