using System;
using System.Collections.Generic;
using System.IO;
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
        public void SendUnauthenticatedMail()
        {
            SendTest(() =>
            {
                CreateNLogConfig();
            }, 1);
        }

        [Fact]
        public void SendAuthenticationMail()
        {
            SendTest(() =>
            {
                // ReSharper disable once StringLiteralTypo
                CreateNLogConfig("user1", "myPassw0rd");
            }, 1);
        }

        [Fact]
        public void SendAuthenticationMail_cc_with_name()
        {
            var transactions = SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.Cc = "no reply <do_not_reply@domain.com>";
            }, 2);

            // 2nd is cc
            AssertMailBox("do_not_reply@domain.com", transactions[0].To[1]);
        }

        private static IList<IMessageTransaction> SendTest(Action createConfig, int toCount)
        {
            var countdownEvent = new CountdownEvent(1);

            var messageStore = new MessageStore(countdownEvent);
            var smtpServer = CreateSmtpServer(messageStore);

            var cancellationToken = new CancellationTokenSource();



            Task.Run(async () => await smtpServer.StartAsync(cancellationToken.Token), cancellationToken.Token);
            {
                createConfig();

                var logger = LogManager.GetLogger("logger1");
                logger.Info("hello first mail!");

                countdownEvent.Wait(TimeSpan.FromSeconds(10));
            }

            cancellationToken.Cancel(false);

            var receivedTransactions = messageStore.ReceivedTransactions;
            Assert.Equal(1, receivedTransactions.Count);
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

        private static MailTarget CreateNLogConfig(string username = null, string password = null)
        {
            var target = new MailTarget("mail1")
            {
                SmtpAuthentication = SmtpAuthenticationMode.None,
                SmtpServer = "localhost",
                To = "mock@mock.com",
                From = "hi@unittest.com",
                SmtpUserName = username,
                SmtpPassword = password
            };

            var loggingConfiguration = new LoggingConfiguration();
            loggingConfiguration.AddRuleForAllLevels(target);

            LogManager.Configuration = loggingConfiguration;

            return target;
        }
    }
}
