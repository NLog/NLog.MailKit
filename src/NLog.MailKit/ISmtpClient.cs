using System;
using System.Net.Security;
using MailKit.Security;
using MimeKit;

namespace NLog.MailKit
{
    internal interface ISmtpClient : IDisposable
    {
        int Timeout { get; set; }
        bool RequireTLS { get; set; }
        RemoteCertificateValidationCallback? ServerCertificateValidationCallback { get; set; }
        void RemoveAuthenticationMechanism(string mechanism);
        void Connect(string host, int port, SecureSocketOptions options);
        void Authenticate(string userName, string password);
        void Authenticate(SaslMechanism mechanism);
        void Send(MimeMessage message);
        void Disconnect(bool quit);
    }
}
