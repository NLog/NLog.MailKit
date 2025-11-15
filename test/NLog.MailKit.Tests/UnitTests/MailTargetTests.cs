using System;
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
        public void MailTargetInitialize_WithSmtpAuthenticationModeNtlm_ThrowsConfigException()
        {
            var mmt = new MailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                SmtpAuthentication = SmtpAuthenticationMode.Ntlm,
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
    }
}
