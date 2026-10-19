using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using DiscussionForum.Models;


namespace DiscussionForum.Models
{
    public class DiscussionBoard
    {
        [Key]
        public int BoardId { get; set; }

        [Required(ErrorMessage = "Course ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be greater than 0.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3,
           ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Description must be between 5 and 500 characters.")]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    }
}
