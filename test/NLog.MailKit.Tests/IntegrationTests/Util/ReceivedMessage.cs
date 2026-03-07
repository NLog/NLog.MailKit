using System.Collections.Generic;
using System.Text;
using SmtpServer;
using SmtpServer.Mail;

namespace NLog.MailKit.Tests.IntegrationTests.Util
{
    public class ReceivedMessage
    {
        private readonly IMessageTransaction _transaction;

        public ReceivedMessage(IMessageTransaction transaction, byte[] body)
        {
            _transaction = transaction;
            Body = body;
        }

        public IMailbox From => _transaction.From;
        public IList<IMailbox> To => _transaction.To;
        public byte[] Body { get; }

        public string GetBodyAsString() => Encoding.UTF8.GetString(Body);
    }
}
