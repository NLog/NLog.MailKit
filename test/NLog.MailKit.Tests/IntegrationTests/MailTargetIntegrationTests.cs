using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            SendTest(port =>
            {
                CreateNLogConfig(port);
            }, 1);
        }

        [Fact]
        public void SendMailWithAuthentication()
        {
            SendTest(port =>
            {
                // ReSharper disable once StringLiteralTypo
                var mailTarget = CreateNLogConfig(port);
                mailTarget.SmtpUserName = "user1";
                mailTarget.SmtpPassword = "myPassw0rd";
            }, 1);
        }

        [Fact]
        public void SendMailWithPickupFolder()
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

        private static IList<ReceivedMessage> SendTest(Action<int> createConfig, int toCount)
        {
            var countdownEvent = new CountdownEvent(1);

            var port = GetFreePort();
            var messageStore = new MessageStore(countdownEvent);
            var smtpServer = CreateSmtpServer(messageStore, port);

            var cancellationToken = new CancellationTokenSource();

            Task.Run(async () => await smtpServer.StartAsync(cancellationToken.Token), cancellationToken.Token);

            WaitForSmtpServer(port);

            createConfig(port);

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

        private static string GetReceivedBody(ReceivedMessage receivedMessage)
        {
            return receivedMessage.GetBodyAsString();
        }

        private static void WaitForSmtpServer(int port, int maxAttempts = 50)
        {
            for (var i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using var client = new TcpClient();
                    client.Connect("127.0.0.1", port);
                    return;
                }
                catch (SocketException)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static SmtpServer.SmtpServer CreateSmtpServer(MessageStore store, int port)
        {
            // ReSharper disable once StringLiteralTypo
            var userAuthenticator = new UserAuthenticator("user1", "myPassw0rd");

            IUserAuthenticatorFactory userAuthenticatorFactory = new UserAuthenticatorFactory(userAuthenticator);
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(port)
                .Build();

            var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            serviceProvider.Add((SmtpServer.Storage.IMessageStoreFactory)store);
            serviceProvider.Add(userAuthenticatorFactory);

            return new SmtpServer.SmtpServer(options, serviceProvider);
        }

        private static MailTarget CreateNLogConfig(int smtpPort = 25)
        {
            var target = new MailTarget("mail1")
            {
                SmtpAuthentication = SmtpAuthenticationMode.None,
                SmtpServer = "localhost",
                SmtpPort = smtpPort,
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
