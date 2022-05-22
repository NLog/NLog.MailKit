using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace NLog.MailKit.Tests.IntegrationTests.Util
{
    public class MessageStore : IMessageStore, IMessageStoreFactory
    {
        public IList<IMessageTransaction> ReceivedTransactions { get; } = new List<IMessageTransaction>();

        private readonly CountdownEvent _countdownEvent;

        public MessageStore(CountdownEvent countdownEvent)
        {
            _countdownEvent = countdownEvent ?? throw new ArgumentNullException(nameof(countdownEvent));
        }

        public IMessageStore CreateInstance(ISessionContext context)
        {
            return this;
        }

        #region Implementation of IMessageStore

        /// <inheritdoc />
        public Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            ReceivedTransactions.Add(transaction);
            _countdownEvent.Signal();
            return Task.FromResult(SmtpResponse.Ok);
        }

        #endregion
    }
}