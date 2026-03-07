namespace NLog.MailKit
{
    /// <summary>
    /// Default factory for creating SMTP clients
    /// </summary>
    internal class SmtpClientFactory : ISmtpClientFactory
    {
        public ISmtpClient Create()
        {
            return new SmtpClientWrapper();
        }
    }
}
