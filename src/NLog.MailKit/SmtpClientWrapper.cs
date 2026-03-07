using System.Net.Security;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NLog.MailKit
{
    internal class SmtpClientWrapper : ISmtpClient
    {
        private readonly global::MailKit.Net.Smtp.ISmtpClient _smtpClient = new SmtpClient();

        public int Timeout
        {
            get => _smtpClient.Timeout;
            set => _smtpClient.Timeout = value;
        }

        public bool RequireTLS
        {
            get => _smtpClient.RequireTLS;
            set => _smtpClient.RequireTLS = value;
        }

        public RemoteCertificateValidationCallback? ServerCertificateValidationCallback
        {
            get => _smtpClient.ServerCertificateValidationCallback;
            set => _smtpClient.ServerCertificateValidationCallback = value;
        }

        public void RemoveAuthenticationMechanism(string mechanism) =>
            _smtpClient.AuthenticationMechanisms.Remove(mechanism);

        public void Connect(string host, int port, SecureSocketOptions options) =>
            _smtpClient.Connect(host, port, options);

        public void Authenticate(string userName, string password) =>
            _smtpClient.Authenticate(userName, password);

        public void Authenticate(SaslMechanism mechanism) =>
            _smtpClient.Authenticate(mechanism);

        public void Send(MimeMessage message) =>
            _smtpClient.Send(message);

        public void Disconnect(bool quit) =>
            _smtpClient.Disconnect(quit);

        public void Dispose() =>
            _smtpClient.Dispose();
    }
}
