
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscussionForum.Models;
using Xunit;

namespace DiscussionForum.Tests
{
    public class DiscussionBoardValidationTests
    {
        private bool IsBoardValid(DiscussionBoard board)
        {
            var context = new ValidationContext(board);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                board, context, results, true);
        }

        private DiscussionBoard CreateValidBoard()
        {
            return new DiscussionBoard
            {
                CourseId = 1,
                Title = "Programming",
                Description = "Programming discussion board"
            };
        }

        // COURSE ID TESTS

        [Fact]
        public void CourseId_One_IsValid()
        {
            var board = CreateValidBoard();

            Assert.True(IsBoardValid(board));
        }

        [Fact]
        public void CourseId_Zero_IsInvalid()
        {
            var board = CreateValidBoard();
            board.CourseId = 0;

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void CourseId_Negative_IsInvalid()
        {
            var board = CreateValidBoard();
            board.CourseId = -1;

            Assert.False(IsBoardValid(board));
        }

        // TITLE TESTS

        [Fact]
        public void Title_Empty_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Title = "";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Title_WhitespaceOnly_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Title = "   ";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Title_TwoCharacters_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Title = "AB";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Title_ThreeCharacters_IsValid()
        {
            var board = CreateValidBoard();
            board.Title = "ABC";

            Assert.True(IsBoardValid(board));
        }

        [Fact]
        public void Title_OneHundredCharacters_IsValid()
        {
            var board = CreateValidBoard();
            board.Title = new string('A', 100);

            Assert.True(IsBoardValid(board));
        }

        [Fact]
        public void Title_OneHundredOneCharacters_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Title = new string('A', 101);

            Assert.False(IsBoardValid(board));
        }

        // DESCRIPTION TESTS

        [Fact]
        public void Description_Empty_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Description = "";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Description_WhitespaceOnly_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Description = "     ";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Description_FourCharacters_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Description = "Test";

            Assert.False(IsBoardValid(board));
        }

        [Fact]
        public void Description_FiveCharacters_IsValid()
        {
            var board = CreateValidBoard();
            board.Description = "Tests";

            Assert.True(IsBoardValid(board));
        }

        [Fact]
        public void Description_FiveHundredCharacters_IsValid()
        {
            var board = CreateValidBoard();
            board.Description = new string('A', 500);

            Assert.True(IsBoardValid(board));
        }

        [Fact]
        public void Description_FiveHundredOneCharacters_IsInvalid()
        {
            var board = CreateValidBoard();
            board.Description = new string('A', 501);

            Assert.False(IsBoardValid(board));
        }
    }
}
