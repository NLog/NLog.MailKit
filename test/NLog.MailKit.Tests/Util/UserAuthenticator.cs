using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Authentication;

namespace NLog.MailKit.Tests.Util
{
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly string _username;
        private readonly string _pasword;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public UserAuthenticator(string username, string pasword)
        {
            _username = username;
            _pasword = pasword;
        }

        
        #region Implementation of IUserAuthenticator

        /// <inheritdoc />
        public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(_username == user && _pasword == password);
        }

        #endregion
    }
}