
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscussionForum.Models;
using Xunit;

namespace DiscussionForum.Tests
{
    public class TopicValidationTests
    {
        private bool IsTopicValid(Topic topic)
        {
            var context = new ValidationContext(topic);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                topic, context, results, true);
        }

        private Topic CreateValidTopic()
        {
            return new Topic
            {
                Title = "Programming",
                Description = "Programming discussion topic",
                BoardId = 1
            };
        }

        // TITLE TESTS

        [Fact]
        public void Title_Valid_IsValid()
        {
            var topic = CreateValidTopic();

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Title_Empty_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Title = "";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Title_WhitespaceOnly_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Title = "   ";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Title_TwoCharacters_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Title = "AB";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Title_ThreeCharacters_IsValid()
        {
            var topic = CreateValidTopic();
            topic.Title = "ABC";

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Title_OneHundredCharacters_IsValid()
        {
            var topic = CreateValidTopic();
            topic.Title = new string('A', 100);

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Title_OneHundredOneCharacters_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Title = new string('A', 101);

            Assert.False(IsTopicValid(topic));
        }

        // DESCRIPTION TESTS

        [Fact]
        public void Description_Valid_IsValid()
        {
            var topic = CreateValidTopic();

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Description_Empty_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Description = "";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Description_WhitespaceOnly_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Description = "     ";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Description_FourCharacters_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Description = "Test";

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void Description_FiveCharacters_IsValid()
        {
            var topic = CreateValidTopic();
            topic.Description = "Tests";

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Description_FiveHundredCharacters_IsValid()
        {
            var topic = CreateValidTopic();
            topic.Description = new string('A', 500);

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void Description_FiveHundredOneCharacters_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.Description = new string('A', 501);

            Assert.False(IsTopicValid(topic));
        }

        // BOARD ID TESTS

        [Fact]
        public void BoardId_One_IsValid()
        {
            var topic = CreateValidTopic();

            Assert.True(IsTopicValid(topic));
        }

        [Fact]
        public void BoardId_Zero_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.BoardId = 0;

            Assert.False(IsTopicValid(topic));
        }

        [Fact]
        public void BoardId_Negative_IsInvalid()
        {
            var topic = CreateValidTopic();
            topic.BoardId = -1;

            Assert.False(IsTopicValid(topic));
        }
    }
}
