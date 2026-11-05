
using DiscussionForum.Data;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionForum.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            var courses = CourseData.Courses;

            return View(courses);
        }
    }
}
