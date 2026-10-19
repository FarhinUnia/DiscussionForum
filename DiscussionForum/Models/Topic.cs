
using System;
using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Models
{
    public class Topic
    {
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Topic title is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Topic description is required.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Description must be between 5 and 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid discussion board.")]
        public int BoardId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DiscussionBoard? DiscussionBoard { get; set; }
    }
}
