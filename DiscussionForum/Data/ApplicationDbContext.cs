using DiscussionForum.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionForum.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DiscussionBoard> DiscussionBoards { get; set; }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<Reply> Replies { get; set; }
    }
}