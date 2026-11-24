
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscussionForum.Models;
using Xunit;

namespace DiscussionForum.Tests
{
    public class ReplyValidationTests
    {
        private bool IsReplyValid(Reply reply)
        {
            var context = new ValidationContext(reply);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(
                reply, context, results, true);
        }

        private Reply CreateValidReply()
        {
            return new Reply
            {
                Content = "This is a valid discussion reply.",
                TopicId = 1
            };
        }

        // CONTENT TESTS

        [Fact]
        public void Content_Valid_IsValid()
        {
            var reply = CreateValidReply();

            Assert.True(IsReplyValid(reply));
        }

        [Fact]
        public void Content_Empty_IsInvalid()
        {
            var reply = CreateValidReply();
            reply.Content = "";

            Assert.False(IsReplyValid(reply));
        }

        [Fact]
        public void Content_WhitespaceOnly_IsInvalid()
        {
            var reply = CreateValidReply();
            reply.Content = "   ";

            // Note: Required does not reject whitespace by itself.
            // This test checks the actual model validation behavior.
            Assert.False(IsReplyValid(reply));
        }

        [Fact]
        public void Content_OneThousandCharacters_IsValid()
        {
            var reply = CreateValidReply();
            reply.Content = new string('A', 1000);

            Assert.True(IsReplyValid(reply));
        }

        [Fact]
        public void Content_OneThousandOneCharacters_IsInvalid()
        {
            var reply = CreateValidReply();
            reply.Content = new string('A', 1001);

            Assert.False(IsReplyValid(reply));
        }

        // TOPIC ID TESTS

        [Fact]
        public void TopicId_One_IsValid()
        {
            var reply = CreateValidReply();

            Assert.True(IsReplyValid(reply));
        }

        // THREADED REPLY TESTS

        [Fact]
        public void ParentReplyId_Null_IsValid()
        {
            var reply = CreateValidReply();
            reply.ParentReplyId = null;

            Assert.True(IsReplyValid(reply));
        }

        [Fact]
        public void ParentReplyId_WithValue_IsValid()
        {
            var reply = CreateValidReply();
            reply.ParentReplyId = 1;

            Assert.True(IsReplyValid(reply));
        }
    }
}
