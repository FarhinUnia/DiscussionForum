using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Models
{
    public class DiscussionBoard
    {
        [Key]
        public int BoardId { get; set; }

        public int CourseId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
