using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Security;
using MimeKit;
using NLog.Config;
using NSubstitute;
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

        [Fact]
        public void SendSimpleMail_UsesSmtpClient()
        {
            var (smtpClient, sentMessages) = CreateSmtpTarget();

            LogManager.GetLogger("logger1").Info("hello first mail!");
            LogManager.Flush();

            smtpClient.Received().Connect(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<SecureSocketOptions>());
            smtpClient.Received().Send(Arg.Any<MimeMessage>());
            smtpClient.Received().Disconnect(true);
            Assert.Single(sentMessages);
            Assert.Contains("hello first mail!", sentMessages[0].Body.ToString());
        }

        [Fact]
        public void SendMailWithAuthentication_CallsAuthenticate()
        {
            var (smtpClient, _) = CreateSmtpTarget(t =>
            {
                t.SmtpUserName = "user1";
                t.SmtpPassword = "password123";
                t.SmtpAuthentication = SmtpAuthenticationMode.Basic;
            });

            LogManager.GetLogger("logger1").Info("test message");
            LogManager.Flush();

            smtpClient.Received().Authenticate("user1", "password123");
        }

        [Fact]
        public void SendMailWithCC_AddsCCRecipients()
        {
            var (_, sentMessages) = CreateSmtpTarget(t => t.Cc = "cc@test.com");

            LogManager.GetLogger("logger1").Info("test message");
            LogManager.Flush();

            Assert.NotNull(sentMessages.FirstOrDefault());
            Assert.NotEmpty(sentMessages[0].Cc);
        }

        [Fact]
        public void SendMailWithPriority_SetsPriority()
        {
            var (_, sentMessages) = CreateSmtpTarget(t => t.Priority = MessagePriority.Urgent.ToString());

            LogManager.GetLogger("logger1").Info("urgent message");
            LogManager.Flush();

            Assert.NotNull(sentMessages.FirstOrDefault());
            Assert.Equal(MessagePriority.Urgent, sentMessages[0].Priority);
        }

        [Fact]
        public void SendMailWithHeaderFooter_IncludesHeaderFooter()
        {
            var (_, sentMessages) = CreateSmtpTarget(t =>
            {
                t.Header = " *** Begin *** ";
                t.Footer = " *** End *** ";
            });

            LogManager.GetLogger("logger1").Info("middle content");
            LogManager.Flush();

            Assert.NotNull(sentMessages.FirstOrDefault());
            var body = sentMessages[0].Body.ToString();
            Assert.Contains("*** Begin ***", body);
            Assert.Contains("*** End ***", body);
            Assert.Contains("middle content", body);
        }

        [Fact]
        public void SendMailBatch_CombinesMessagesInBody()
        {
            var sentMessages = new List<MimeMessage>();
            var smtpClient = Substitute.For<ISmtpClient>();
            smtpClient.When(x => x.Send(Arg.Any<MimeMessage>())).Do(x => sentMessages.Add(x.Arg<MimeMessage>()));
            var factory = Substitute.For<ISmtpClientFactory>();
            factory.Create().Returns(smtpClient);

            var target = new MailTarget("mail1") { SmtpServer = "localhost", SmtpPort = 25, To = "mock@mock.com", From = "hi@unittest.com" };
            target.SmtpClientFactory = factory;
            var config = new LoggingConfiguration();
            config.AddRuleForAllLevels(new NLog.Targets.Wrappers.BufferingTargetWrapper(target));
            LogManager.Configuration = config;

            var logger = LogManager.GetLogger("logger1");
            logger.Info("hello starting mail!");
            logger.Info("hello first mail!");
            LogManager.Flush();

            Assert.Single(sentMessages);
            var body = sentMessages[0].Body.ToString();
            Assert.Contains("hello starting mail!", body);
            Assert.Contains("hello first mail!", body);
        }

        [Fact]
        public void SendMail_WithSkipCertificateValidation_SetsCertificateCallback()
        {
            var (smtpClient, _) = CreateSmtpTarget(t => t.SkipCertificateValidation = true);

            LogManager.GetLogger("logger1").Info("test message");
            LogManager.Flush();

            Assert.NotNull(smtpClient.ServerCertificateValidationCallback);
        }

        [Fact]
        public void SendMail_WithRequireTLS_SetsRequireTLS()
        {
            var (smtpClient, _) = CreateSmtpTarget(t => t.RequireTLS = true);

            LogManager.GetLogger("logger1").Info("test message");
            LogManager.Flush();

            Assert.True(smtpClient.RequireTLS);
        }

        [Fact]
        public void SendMailWithCustomHeader_AddsHeaderToMessage()
        {
            var (_, sentMessages) = CreateSmtpTarget(t =>
                t.MailHeaders.Add(new Targets.MethodCallParameter("FooHeader", "bar")));

            LogManager.GetLogger("logger1").Info("test message");
            LogManager.Flush();

            Assert.NotNull(sentMessages.FirstOrDefault());
            Assert.NotEmpty(sentMessages[0].Headers["FooHeader"]);
        }

        private static (ISmtpClient smtpClient, List<MimeMessage> sentMessages) CreateSmtpTarget(
            Action<MailTarget>? configure = null)
        {
            var sentMessages = new List<MimeMessage>();
            var smtpClient = Substitute.For<ISmtpClient>();
            smtpClient.When(x => x.Send(Arg.Any<MimeMessage>())).Do(x => sentMessages.Add(x.Arg<MimeMessage>()));
            var factory = Substitute.For<ISmtpClientFactory>();
            factory.Create().Returns(smtpClient);

            var target = new MailTarget("mail1")
            {
                SmtpAuthentication = SmtpAuthenticationMode.None,
                SmtpServer = "localhost",
                SmtpPort = 25,
                To = "mock@mock.com",
                From = "hi@unittest.com",
            };
            target.SmtpClientFactory = factory;
            configure?.Invoke(target);

            var config = new LoggingConfiguration();
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;

            return (smtpClient, sentMessages);
        }
    }
}
