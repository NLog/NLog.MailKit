using SmtpServer;
using SmtpServer.Authentication;

namespace NLog.MailKit.Tests.Util
{
    internal class UserAuthenticatorFactory : IUserAuthenticatorFactory
    {
        private readonly IUserAuthenticator _authenticator;

        public UserAuthenticatorFactory(IUserAuthenticator authenticator)
        {
            _authenticator = authenticator;
        }

        #region Implementation of IUserAuthenticatorFactory

        /// <inheritdoc />
        public IUserAuthenticator CreateInstance(ISessionContext context)
        {
            return _authenticator;
        }

        #endregion
    }
}