using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog.Config;
using NLog.MailKit.Tests.IntegrationTests.Util;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Mail;
using Xunit;
using UserAuthenticator = NLog.MailKit.Tests.IntegrationTests.Util.UserAuthenticator;

namespace NLog.MailKit.Tests.IntegrationTests
{
    public class MailTargetIntegrationTests
    {
        [Fact]
        public void SendSimpleMail()
        {
            SendTest(() =>
            {
                CreateNLogConfig();
            }, 1);
        }

        [Fact]
        public void SendMailWihAuthentication()
        {
            SendTest(() =>
            {
                // ReSharper disable once StringLiteralTypo
                var mailTarget = CreateNLogConfig();
                mailTarget.SmtpUserName = "user1";
                mailTarget.SmtpPassword = "myPassw0rd";
            }, 1);
        }

        [Fact]
        public void SendMailWithCC()
        {
            var transactions = SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.Cc = "no reply <do_not_reply@domain.com>";
            }, 2);

            // 2nd is cc
            AssertMailBox("do_not_reply@domain.com", transactions[0].To[1]);
        }

        [Fact]
        public void SendMailWithPriority()
        {
            SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.Priority = MimeKit.MessagePriority.Urgent.ToString();
            }, 1);
        }

        [Fact]
        public void SendMailWithHeader()
        {
            SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.MailHeaders.Add(new Targets.MethodCallParameter("FooHeader", ""));
            }, 1);
        }

        [Fact]
        public void SendMailWithHeaderFooter()
        {
            var transactions = SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.Header = " *** Begin *** ";
                mailTarget.Footer = " *** End *** ";
            }, 1);

            var mailMessage = transactions.LastOrDefault()?.Message as SmtpServer.Mail.ITextMessage;
            Assert.NotNull(mailMessage);
            mailMessage.Content.Position = 0;
            var mailBody = new StreamReader(mailMessage.Content).ReadToEnd();
            Assert.NotNull(mailBody);
            Assert.Contains("*** Begin ***", mailBody);
            Assert.Contains("*** End ***", mailBody);
        }

        [Fact]
        public void SendMailWitPickupFolder()
        {
            // Arrange
            var tempFolder = Path.Combine(Path.GetTempPath(), "NLog_MailKit_" + Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempFolder);

                // Act
                var mailTarget = CreateNLogConfig();
                mailTarget.PickupDirectoryLocation = tempFolder;
                var logger = LogManager.GetLogger("logger1");
                var expectedMessage = "hello first mail!";
                logger.Info(expectedMessage);

                // Assert 
                var files = Directory.GetFiles(tempFolder);
                Assert.Single(files);
                var msg = MimeKit.MimeMessage.Load(files[0]);
                Assert.Contains(expectedMessage, msg.Body.ToString());
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }
        }

        [Fact]
        public void SendMailBatch()
        {
            var transactions = SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                var loggingConfiguration = new LoggingConfiguration();
                loggingConfiguration.AddRuleForAllLevels(new NLog.Targets.Wrappers.BufferingTargetWrapper(mailTarget));
                NLog.LogManager.Configuration = loggingConfiguration;

                var logger = LogManager.GetLogger("logger1");
                logger.Info("hello starting mail!");
            }, 1);

            var mailMessage = transactions.LastOrDefault()?.Message as SmtpServer.Mail.ITextMessage;
            Assert.NotNull(mailMessage);
            mailMessage.Content.Position = 0;
            var mailBody = new StreamReader(mailMessage.Content).ReadToEnd();
            Assert.NotNull(mailBody);
            Assert.Contains("hello starting mail!", mailBody);
            Assert.Contains("hello first mail!", mailBody);
        }

        private static IList<IMessageTransaction> SendTest(Action createConfig, int toCount)
        {
            var countdownEvent = new CountdownEvent(1);

            var messageStore = new MessageStore(countdownEvent);
            var smtpServer = CreateSmtpServer(messageStore);

            var cancellationToken = new CancellationTokenSource();

            Task.Run(async () => await smtpServer.StartAsync(cancellationToken.Token), cancellationToken.Token);

            createConfig();

            var logger = LogManager.GetLogger("logger1");
            logger.Info("hello first mail!");

            LogManager.Flush();

            countdownEvent.Wait(TimeSpan.FromSeconds(10));
            cancellationToken.Cancel(false);

            var receivedTransactions = messageStore.ReceivedTransactions;
            Assert.Single(receivedTransactions);
            var receivedMessage = receivedTransactions[0];
            AssertMailBox("hi@unittest.com", receivedMessage.From);
            var receivedBody = GetReceivedBody(receivedMessage);
            Assert.Contains("hello first mail!", receivedBody);

            Assert.Equal(toCount, receivedMessage.To.Count);

            return receivedTransactions;
        }

        private static void AssertMailBox(string expected, IMailbox mailbox)
        {
            Assert.Equal(expected, mailbox.User + "@" + mailbox.Host);
        }

        private static string GetReceivedBody(IMessageTransaction receivedMessage)
        {
            if (receivedMessage.Message is ITextMessage textMessage)
            {
                var streamReader = new StreamReader(textMessage.Content);
                return streamReader.ReadToEnd();
            }
            throw new NotSupportedException("only ITextMessage supported");
        }

        private static SmtpServer.SmtpServer CreateSmtpServer(MessageStore store)
        {
            // ReSharper disable once StringLiteralTypo
            var userAuthenticator = new UserAuthenticator("user1", "myPassw0rd");

            IUserAuthenticatorFactory userAuthenticatorFactory = new UserAuthenticatorFactory(userAuthenticator);
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25, 587)
                .MessageStore(store)
                .UserAuthenticator(userAuthenticatorFactory)
                .Build();

            var smtpServer = new SmtpServer.SmtpServer(options);
            return smtpServer;
        }

        private static MailTarget CreateNLogConfig()
        {
            var target = new MailTarget("mail1")
            {
                SmtpAuthentication = SmtpAuthenticationMode.None,
                SmtpServer = "localhost",
                To = "mock@mock.com",
                From = "hi@unittest.com",
            };

            var loggingConfiguration = new LoggingConfiguration();
            loggingConfiguration.AddRuleForAllLevels(target);

            LogManager.Configuration = loggingConfiguration;

            return target;
        }
    }
}
