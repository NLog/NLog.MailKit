using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog.Config;
using NLog.Targets;
using SmtpServer;
using SmtpServer.Mail;
using Xunit;

namespace NLog.MailKit.Tests
{
    public class MailTargetTests
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
            SendTest(() =>
            {
                var mailTarget = CreateNLogConfig();
                mailTarget.Cc = "no reply <do_not_reply@domain.com>";
            }, 2, (message) =>
            {
                var fullMessage = message.Mime.ToString();
                // wont work, bug ? Assert.Equal("no reply", bcc.DisplayName);
                Assert.Contains("Cc: no reply <do_not_reply@domain.com>", fullMessage);

            });
        }

        private static void SendTest(Action createConfig, int toCount, Action<IMimeMessage> extraTest = null)
        {
            var countdownEvent = new CountdownEvent(1);

            var messageStore = new MessageStore(countdownEvent);
            var smtpServer = CreateSmtpServer(messageStore);

            var cancellationToken = new CancellationTokenSource();
            smtpServer.StartAsync(cancellationToken.Token);

            createConfig();


            var logger = LogManager.GetLogger("logger1");
            logger.Info("hello first mail!");

            countdownEvent.Wait(TimeSpan.FromSeconds(50));

            Assert.Equal(1, messageStore.RecievedMessages.Count);
            var recievedMesssage = messageStore.RecievedMessages[0];
            Assert.Equal("hi@unittest.com", recievedMesssage.From.User + "@" + recievedMesssage.From.Host);
            var recievedBody = recievedMesssage.Mime.ToString();
            Assert.Contains("hello first mail!", recievedBody);

            Assert.Equal(toCount, recievedMesssage.To.Count);

            extraTest?.Invoke(recievedMesssage);

            cancellationToken.Cancel(false);
        }

        private static SmtpServer.SmtpServer CreateSmtpServer(MessageStore store)
        {
            var userAuthenticator = new UserAuthenticator("user1", "myPassw0rd");
            var options = new OptionsBuilder()
                .ServerName("localhost")
                .Port(25, 587)
                .MessageStore(store)
                .AllowUnsecureAuthentication(false)
                .UserAuthenticator(userAuthenticator)
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
