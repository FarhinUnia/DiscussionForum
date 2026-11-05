using System.Collections.Generic;

namespace DiscussionForum.Data
{
    public static class CourseData
    {
        public static List<CourseInfo> Courses = new List<CourseInfo>
        {
            new CourseInfo
            {
                CourseId = 1,
                CourseCode = "PROG73010",
                CourseName = "Introduction to Big Data"
            },

            new CourseInfo
            {
                CourseId = 2,
                CourseCode = "CSCN73030",
                CourseName = "Project V: Advanced Software Development"
            },

            new CourseInfo
            {
                CourseId = 3,
                CourseCode = "RSCH73000",
                CourseName = "Understanding Research"
            },

            new CourseInfo
            {
                CourseId = 4,
                CourseCode = "CSCN73000",
                CourseName = "Parallel Computing Techniques"
            },

            new CourseInfo
            {
                CourseId = 5,
                CourseCode = "SECU73000",
                CourseName = "Introduction to Software Security"
            },

            new CourseInfo
            {
                CourseId = 6,
                CourseCode = "CSCN73010",
                CourseName = "Software Quality III: Build and Release"
            }
        };
    }
}