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

        /// <summary>
        /// OAuth 2.0 (XOAUTH2) Authentication
        /// </summary>
        OAuth2
    }
}
