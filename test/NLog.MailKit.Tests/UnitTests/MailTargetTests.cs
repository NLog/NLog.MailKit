using NLog.MailKit;
using System;
using MimeKit;
using Xunit;

namespace NLog.MailKit.Tests
{
    public class MailTargetTests
    {
        [Theory]
        [InlineData("high", MessagePriority.Urgent)]
        [InlineData("HIGH", MessagePriority.Urgent)]
        [InlineData(" HIGH ", MessagePriority.Urgent)]
        [InlineData("low", MessagePriority.NonUrgent)]
        [InlineData("LOW", MessagePriority.NonUrgent)]
        [InlineData(" LOW ", MessagePriority.NonUrgent)]
        [InlineData("Urgent", MessagePriority.Urgent)]
        [InlineData("URGENT", MessagePriority.Urgent)]
        [InlineData(" URGENT ", MessagePriority.Urgent)]
        [InlineData("normal", MessagePriority.Normal)]
        public void ParseMessagePriorityTests(string input, MessagePriority expected)
        {
            // Act
            var result = MailTarget.ParseMessagePriority(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
