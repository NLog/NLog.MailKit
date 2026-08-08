using System;
using System.Linq;
using MimeKit;
using Xunit;

namespace NLog.MailKit.Tests.UnitTests
{
    public class MailTargetTests
    {
        public MailTargetTests()
        {
            NLog.LogManager.ThrowConfigExceptions = true;
        }

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

        [Fact]
        public void MailTarget_WithEmptyTo_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = "",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTarget_WithBlankTo_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = " ",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTarget_WithInvalidTo_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = "@",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTarget_WithEmptyFrom_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "",
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTarget_WithEmptySmtpServer_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "bar@bar.com",
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedTo_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedFrom_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedSmtpServer_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpPort = 27,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Fact]
        public void MailTargetInitialize_WithSmtpAuthenticationModeOAuth2_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                SmtpAuthentication = SmtpAuthenticationMode.OAuth2,
            };

            Assert.Throws<NLogConfigurationException>(() =>
                new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(mmt);
                })
            );
        }

        [Theory]
        [InlineData("Line 1\r\nLine 2", true)]
        [InlineData("Line 1\r\nLine 2", false)]
        [InlineData("Line 1\nLine 2", true)]
        [InlineData("Line 1\nLine 2", false)]
        [InlineData("Line 1\r\nLine 2\nLine 3", true)]
        [InlineData("Line 1\r\nLine 2\nLine 3", false)]
        public void MailTarget_ReplaceNewlineWithBrTagInHtml_ReplacesAllNewlineFormats(string body, bool replaceNewLine)
        {
            // Arrange
            MimeMessage sentMessage = null;
            Action<MimeMessage, LogEventInfo> mailSender = (message, logEvent) => { sentMessage = message; };
            var target = new MailTarget(mailSender)
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Test",
                SmtpServer = "does-not-matter",
                Body = "${message}",
                Html = true,
                ReplaceNewlineWithBrTagInHtml = replaceNewLine,
            };

            // Act
            using var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(target);
            }).LogFactory;
            var logger = logFactory.GetLogger("logger1");
            logger.Info(body);

            // Assert
            Assert.NotNull(sentMessage);
            var textPart = Assert.IsType<TextPart>(sentMessage.Body);
            Assert.Equal(MimeKit.Text.TextFormat.Html, textPart.Format);
            if (replaceNewLine)
            {
                Assert.Contains("<br/>", textPart.Text);
                Assert.DoesNotContain("\n", textPart.Text);
            }
            else
            {
                Assert.DoesNotContain("<br/>", textPart.Text);
                Assert.Contains("\n", textPart.Text);
            }
        }

        [Fact]
        public void SendMailWithCC()
        {
            // Arrange
            MimeMessage sentMessage = null;
            Action<MimeMessage, LogEventInfo> mailSender = (message, logEvent) => { sentMessage = message; };
            
            var target = new MailTarget(mailSender)
            {
                From = "hi@unittest.com",
                To = "mock@mock.com",
                Cc = "no reply <do_not_reply@domain.com>",
                Subject = "Test",
                SmtpServer = "does-not-matter",
            };
            using var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => { cfg.Configuration.AddRuleForAllLevels(target); }).LogFactory;
            // Act
            logFactory.GetLogger("logger1").Info("hello first mail!");
            // Assert
            Assert.NotNull(sentMessage);
            Assert.Single(sentMessage.To);
            Assert.Single(sentMessage.Cc);
            Assert.Equal("mock@mock.com", sentMessage.To.Mailboxes.Single().Address);
            Assert.Equal("do_not_reply@domain.com", sentMessage.Cc.Mailboxes.Single().Address);
        }

        [Fact]
        public void SendMailWithPriority()
        {
            // Arrange
            MimeMessage sentMessage = null;
            Action<MimeMessage, LogEventInfo> mailSender = (message, logEvent) => { sentMessage = message; };
            var target = new MailTarget(mailSender)
            {
                From = "hi@unittest.com",
                To = "mock@mock.com",
                Priority = MimeKit.MessagePriority.Urgent.ToString(),
                Subject = "Test",
                SmtpServer = "does-not-matter",
            };
            using var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => { cfg.Configuration.AddRuleForAllLevels(target); }).LogFactory;
            // Act
            logFactory.GetLogger("logger1").Info("hello first mail!");
            // Assert
            Assert.NotNull(sentMessage);
            Assert.Equal(MimeKit.MessagePriority.Urgent, sentMessage.Priority);
        }

        [Fact]
        public void SendMailWithHeaderFooter()
        {
            // Arrange
            MimeMessage sentMessage = null;
            Action<MimeMessage, LogEventInfo> mailSender = (message, logEvent) => { sentMessage = message; };
            var target = new MailTarget(mailSender)
            {
                From = "hi@unittest.com",
                To = "mock@mock.com",
                Subject = "Test",
                Header = " *** Begin *** ",
                Footer = " *** End *** ",
                SmtpServer = "does-not-matter",
            };
            using var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => { cfg.Configuration.AddRuleForAllLevels(target); }).LogFactory;
            // Act
            logFactory.GetLogger("logger1").Info("hello first mail!");
            // Assert
            Assert.NotNull(sentMessage);
            var mailBody = sentMessage.Body.ToString();
            Assert.NotNull(mailBody);
            Assert.Contains("*** Begin ***", mailBody);
            Assert.Contains("hello first mail!", mailBody);
            Assert.Contains("*** End ***", mailBody);
        }

        [Fact]
        public void SendMailWithHeaders()
        {
            // Arrange
            MimeMessage sentMessage = null;
            Action<MimeMessage, LogEventInfo> mailSender = (message, logEvent) => { sentMessage = message; };
            var target = new MailTarget(mailSender)
            {
                From = "hi@unittest.com",
                To = "mock@mock.com",
                Subject = "Test",
                SmtpServer = "does-not-matter",
            };
            target.MailHeaders.Add(new Targets.MethodCallParameter("FooHeader", "FooValue"));
            using var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => { cfg.Configuration.AddRuleForAllLevels(target); }).LogFactory;
            // Act
            logFactory.GetLogger("logger1").Info("hello first mail!");
            // Assert
            Assert.NotNull(sentMessage);
            Assert.Equal("FooValue", sentMessage.Headers["FooHeader"]);
        }
    }
}
