namespace NLog.MailKit
{
    internal interface ISmtpClientFactory
    {
        ISmtpClient Create();
    }
}
