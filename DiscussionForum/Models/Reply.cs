
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiscussionForum.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }

        [Required(ErrorMessage = "Reply cannot be empty.")]
        [StringLength(1000, MinimumLength = 1,
            ErrorMessage = "Reply must be between 1 and 1000 characters.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int TopicId { get; set; }

        [JsonIgnore]
        public Topic? Topic { get; set; }

        // Null means a direct reply to the topic.
        // A value means a nested reply.
        public int? ParentReplyId { get; set; }

        [JsonIgnore]
        public Reply? ParentReply { get; set; }

        [JsonIgnore]
        public ICollection<Reply> ChildReplies { get; set; }
            = new List<Reply>();
    }
}
