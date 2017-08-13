using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace NLog.MailKit.Tests
{
    public class MessageStore : IMessageStore, IMessageStoreFactory
    {
        public IList<IMessageTransaction> RecievedMessages { get; }

        private readonly CountdownEvent _countdownEvent;

        public MessageStore(CountdownEvent countdownEvent)
        {
            if (countdownEvent == null)
            {
                throw new ArgumentNullException(nameof(countdownEvent));
            }
            RecievedMessages = new List<IMessageTransaction>();
            _countdownEvent = countdownEvent;
        }

        public IMessageStore CreateInstance(ISessionContext context)
        {
            return this;
        }

        #region Implementation of IMessageStore

        public Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction message, CancellationToken cancellationToken)
        {
            RecievedMessages.Add(message);
            _countdownEvent.Signal();
            return Task.FromResult(SmtpResponse.Ok);
        }

        #endregion
    }
}