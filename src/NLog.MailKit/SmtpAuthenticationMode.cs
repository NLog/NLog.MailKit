using System;

namespace NLog.MailKit
{
    /// <summary>
    /// SMTP authentication modes.
    /// </summary>
    public enum SmtpAuthenticationMode
    {
        /// <summary>
        /// No authentication.
        /// </summary>
        None,

        /// <summary>
        /// Basic - username and password.
        /// </summary>
        Basic,

        /// <summary>
        /// NTLM Authentication.
        /// </summary>
        Ntlm,
    }
}
