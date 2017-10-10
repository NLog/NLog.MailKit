using System;
using System.Collections.Generic;
using System.Text;

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
