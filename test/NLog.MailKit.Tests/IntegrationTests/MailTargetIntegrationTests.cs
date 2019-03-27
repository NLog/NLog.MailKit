using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog.Config;
using NLog.MailKit.Tests.Util;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Mail;
using Xunit;
using UserAuthenticator = NLog.MailKit.Tests.Util.UserAuthenticator;

namespace NLog.MailKit.Tests.IntegrationTests
{
    public class MailTargetIntegrationTests
    {
        [Fact]
        public void SendUnauthenticationMail()
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

            //2nd is cc
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

            var recievedTransactions = messageStore.RecievedTransactions;
            Assert.Equal(1, recievedTransactions.Count);
            var recievedMesssage = recievedTransactions[0];
            AssertMailBox("hi@unittest.com", recievedMesssage.From);
            var recievedBody = GetRecievedBody(recievedMesssage);
            Assert.Contains("hello first mail!", recievedBody);

            Assert.Equal(toCount, recievedMesssage.To.Count);

            return recievedTransactions;

        }

        private static void AssertMailBox(string expected, IMailbox mailbox)
        {
            Assert.Equal(expected, mailbox.User + "@" + mailbox.Host);
        }

        private static string GetRecievedBody(IMessageTransaction recievedMesssage)
        {
            if (recievedMesssage.Message is ITextMessage textMessage)
            {
                var streamReader = new StreamReader(textMessage.Content);
                return streamReader.ReadToEnd();
            }
            throw new NotSupportedException("only ITextMessage supported");
        }

        private static SmtpServer.SmtpServer CreateSmtpServer(MessageStore store)
        {
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
