// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.MailKit
{
    /// <summary>
    /// Sends log messages by email using SMTP protocol with MailKit.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Mail-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Simple/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Simple/Example.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple log messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Buffered/NLog.config" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Buffered/Example.cs" />
    /// </example>
    [Target("Mail")]
    public class MailTarget : TargetWithLayoutHeaderAndFooter
    {
        private const string RequiredPropertyIsEmptyFormat = "After the processing of the MailTarget's '{0}' property it appears to be empty. The email message will not be sent.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This one is safe.")]
        public MailTarget()
        {
            Body = "${message}${newline}";
            Subject = "Message from NLog on ${machinename}";
            Encoding = Encoding.UTF8;
            SmtpPort = 25;
            SmtpAuthentication = SmtpAuthenticationMode.None;
            SecureSocketOption = SecureSocketOptions.StartTlsWhenAvailable;
            Timeout = 10000;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public MailTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets sender's email address (e.g. joe@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='10' />
        public Layout From { get; set; }

        /// <summary>
        /// Gets or sets recipients' email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='11' />
        [RequiredParameter]
        public Layout To { get; set; }

        /// <summary>
        /// Gets or sets CC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='12' />
        public Layout Cc { get; set; }

        /// <summary>
        /// Gets or sets BCC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='13' />
        public Layout Bcc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add new lines between log entries.
        /// </summary>
        /// <value>A value of <c>true</c> if new lines should be added; otherwise, <c>false</c>.</value>
        /// <docgen category='Layout Options' order='99' />
        public bool AddNewLines { get; set; }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        /// <docgen category='Message Options' order='5' />
        [DefaultValue("Message from NLog on ${machinename}")]
        [RequiredParameter]
        public Layout Subject { get; set; }

        /// <summary>
        /// Gets or sets mail message body (repeated for each log message send in one mail).
        /// </summary>
        /// <remarks>Alias for the <c>Layout</c> property.</remarks>
        /// <docgen category='Message Options' order='6' />
        [DefaultValue("${message}${newline}")]
        public Layout Body
        {
            get => Layout;
            set => Layout = value;
        }

        /// <summary>
        /// Gets or sets encoding to be used for sending e-mail.
        /// </summary>
        /// <docgen category='Layout Options' order='20' />
        [DefaultValue("UTF8")]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send message as HTML instead of plain text.
        /// </summary>
        /// <docgen category='Layout Options' order='11' />
        [DefaultValue(false)]
        public bool Html { get; set; }

        /// <summary>
        /// Gets or sets SMTP Server to be used for sending.
        /// </summary>
        /// <docgen category='SMTP Options' order='10' />
        public Layout SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets SMTP Authentication mode.
        /// </summary>
        /// <docgen category='SMTP Options' order='11' />
        [DefaultValue("None")]
        public SmtpAuthenticationMode SmtpAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the username used to connect to SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='12' />
        public Layout SmtpUserName { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate against SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='13' />
        public Layout SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL (secure sockets layer) should be used when communicating with SMTP server.
        /// 
        /// See also <see cref="SecureSocketOption"/>
        /// </summary>
        /// <docgen category='SMTP Options' order='14' />.
        [DefaultValue(false)]
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Provides a way of specifying the SSL and/or TLS encryption 
        /// 
        /// If <see cref="EnableSsl"/> is <c>true</c>, then <see cref="SecureSocketOptions.SslOnConnect"/> will be used.
        /// </summary>
        [DefaultValue(SecureSocketOptions.StartTlsWhenAvailable)]
        public SecureSocketOptions SecureSocketOption { get; set; }

        /// <summary>
        /// Gets or sets the port number that SMTP Server is listening on.
        /// </summary>
        /// <docgen category='SMTP Options' order='15' />
        [DefaultValue(25)]
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SmtpClient should ignore invalid certificate.
        /// </summary>
        /// <docgen category='SMTP Options' order='16' />.
        [DefaultValue(false)]
        public bool SkipCertificateValidation { get; set; }
        
        /// <summary>
        /// Gets or sets the priority used for sending mails.
        /// </summary>
        public Layout Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether NewLine characters in the body should be replaced with <br/> tags.
        /// </summary>
        /// <remarks>Only happens when <see cref="Html"/> is set to true.</remarks>
        [DefaultValue(false)]
        public bool ReplaceNewlineWithBrTagInHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the SMTP client timeout.
        /// </summary>
        /// <remarks>Warning: zero is not infinit waiting</remarks>
        [DefaultValue(10000)]
        public int Timeout { get; set; }

        /// <summary>
        /// Renders the logging event message and adds it to the internal ArrayList of log messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write((IList<AsyncLogEventInfo>)new[] { logEvent });
        }

#if !NETSTANDARD2_0

        /// <summary>
        /// NOTE! Will soon be marked obsolete. Instead override Write(IList{AsyncLogEventInfo} logEvents)
        /// 
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            Write((IList<AsyncLogEventInfo>)logEvents);
        }

#endif

        /// <summary>
        /// Renders an array logging events.
        /// </summary>
        /// <param name="logEvents">Array of logging events.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            var buckets = logEvents.BucketSort(c => GetSmtpSettingsKey(c.LogEvent));
            foreach (var bucket in buckets)
            {
                var eventInfos = bucket.Value;
                ProcessSingleMailMessage(eventInfos);
            }
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            InternalLogger.Debug("Init mailtarget with mailkit");
            CheckRequiredParameters();

            if (this.SmtpAuthentication == SmtpAuthenticationMode.Ntlm)
            {
                throw new NLogConfigurationException("Ntlm not yet supported");
            }

            base.InitializeTarget();
        }



        /// <summary>
        /// Create mail and send with SMTP
        /// </summary>
        /// <param name="events">event printed in the body of the event</param>
        private void ProcessSingleMailMessage(IList<AsyncLogEventInfo> events)
        {
            try
            {
                if (events.Count == 0)
                {
                    throw new NLogRuntimeException("We need at least one event.");
                }

                LogEventInfo firstEvent = events[0].LogEvent;
                LogEventInfo lastEvent = events[events.Count - 1].LogEvent;

                // unbuffered case, create a local buffer, append header, body and footer
                var bodyBuffer = CreateBodyBuffer(events, firstEvent, lastEvent);

                var message = CreateMailMessage(lastEvent, bodyBuffer.ToString());

                using (var client = new SmtpClient())
                {
                    CheckRequiredParameters();
                    client.Timeout = Timeout;

                    var renderedHost = SmtpServer.Render(lastEvent);
                    if (string.IsNullOrEmpty(renderedHost))
                    {
                        throw new NLogRuntimeException(RequiredPropertyIsEmptyFormat, nameof(SmtpServer));
                    }

                    var secureSocketOptions = EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOption;
                    InternalLogger.Debug("Sending mail to {0} using {1}:{2} (socket option={3})", message.To, renderedHost, SmtpPort, secureSocketOptions);
                    InternalLogger.Trace("  Subject: '{0}'", message.Subject);
                    InternalLogger.Trace("  From: '{0}'", message.From.ToString());
                    
                    if(SkipCertificateValidation)
                        client.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;

                   
                    client.Connect(renderedHost, SmtpPort, secureSocketOptions);
                    InternalLogger.Trace("  Connecting succesfull");

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication

                    if (this.SmtpAuthentication == SmtpAuthenticationMode.Basic)
                    {
                        var userName = SmtpUserName?.Render(lastEvent);
                        var password = SmtpPassword?.Render(lastEvent);

                        InternalLogger.Debug("Authenticate with username '{0}'", userName);
                        client.Authenticate(userName, password);
                    }

                    client.Send(message);
                    InternalLogger.Debug("Sending mail done. Disconnecting");
                    client.Disconnect(true);
                    InternalLogger.Debug("Disconnected");

                    foreach (var ev in events)
                    {
                        ev.Continuation(null);
                    }
                }
            }
            catch (Exception exception)
            {
                //always log
                InternalLogger.Error(exception, "Error sending mail.");

                if (exception.MustBeRethrown())
                {
                    throw;
                }


                foreach (var ev in events)
                {
                    ev.Continuation(exception);
                }
            }
        }

        /// <summary>
        /// Create buffer for body
        /// </summary>
        /// <param name="events">all events</param>
        /// <param name="firstEvent">first event for header</param>
        /// <param name="lastEvent">last event for footer</param>
        /// <returns></returns>
        private StringBuilder CreateBodyBuffer(IEnumerable<AsyncLogEventInfo> events, LogEventInfo firstEvent, LogEventInfo lastEvent)
        {
            var bodyBuffer = new StringBuilder();
            if (Header != null)
            {
                bodyBuffer.Append(Header.Render(firstEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            foreach (AsyncLogEventInfo eventInfo in events)
            {
                bodyBuffer.Append(Layout.Render(eventInfo.LogEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            if (Footer != null)
            {
                bodyBuffer.Append(Footer.Render(lastEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }
            return bodyBuffer;
        }

        private void CheckRequiredParameters()
        {
            if (SmtpServer == null)
            {
                throw new NLogConfigurationException(RequiredPropertyIsEmptyFormat, nameof(SmtpServer));
            }

            if (From == null)
            {
                throw new NLogConfigurationException(RequiredPropertyIsEmptyFormat, nameof(From));
            }
        }

        /// <summary>
        /// Create key for grouping. Needed for multiple events in one mailmessage
        /// </summary>
        /// <param name="logEvent">event for rendering layouts   </param>  
        ///<returns>string to group on</returns>
        private string GetSmtpSettingsKey(LogEventInfo logEvent)
        {
            var sb = new StringBuilder();

            AppendLayout(sb, logEvent, From);
            AppendLayout(sb, logEvent, To);
            AppendLayout(sb, logEvent, Cc);
            AppendLayout(sb, logEvent, Bcc);
            AppendLayout(sb, logEvent, SmtpServer);
            AppendLayout(sb, logEvent, SmtpPassword);
            AppendLayout(sb, logEvent, SmtpUserName);


            return sb.ToString();
        }

        /// <summary>
        /// Append rendered layout to the stringbuilder
        /// </summary>
        /// <param name="sb">append to this</param>
        /// <param name="logEvent">event for rendering <paramref name="layout"/></param>
        /// <param name="layout">append if not <c>null</c></param>
        private static void AppendLayout(StringBuilder sb, LogEventInfo logEvent, Layout layout)
        {
            sb.Append("|");
            if (layout != null)
                sb.Append(layout.Render(logEvent));
        }



        /// <summary>
        /// Create the mailmessage with the addresses, properties and body.
        /// </summary>
        private MimeMessage CreateMailMessage(LogEventInfo lastEvent, string body)
        {
            var msg = new MimeMessage();

            var renderedFrom = From?.Render(lastEvent);

            if (string.IsNullOrEmpty(renderedFrom))
            {
                throw new NLogRuntimeException(RequiredPropertyIsEmptyFormat, "From");
            }
            msg.From.Add(MailboxAddress.Parse(renderedFrom));

            var addedTo = AddAddresses(msg.To, To, lastEvent);
            var addedCc = AddAddresses(msg.Cc, Cc, lastEvent);
            var addedBcc = AddAddresses(msg.Bcc, Bcc, lastEvent);

            if (!addedTo && !addedCc && !addedBcc)
            {
                throw new NLogRuntimeException(RequiredPropertyIsEmptyFormat, "To/Cc/Bcc");
            }

            msg.Subject = Subject == null ? string.Empty : Subject.Render(lastEvent).Trim();

            //todo msg.BodyEncoding = Encoding;

            if (Priority != null)
            {
                var renderedPriority = Priority.Render(lastEvent);
                try
                {

                    msg.Priority = (MessagePriority)Enum.Parse(typeof(MessagePriority), renderedPriority, true);
                }
                catch
                {
                    InternalLogger.Warn("Could not convert '{0}' to MessagePriority, valid values are NonUrgent, Normal and Urgent. Using normal priority as fallback.");
                    msg.Priority = MessagePriority.Normal;
                }
            }

            TextPart CreateBodyPart()
            {
                var newBody = body;
                if (Html && ReplaceNewlineWithBrTagInHtml)
                    newBody = newBody?.Replace(Environment.NewLine, "<br/>");
                return new TextPart(Html ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = newBody,
                    ContentType = { Charset = Encoding?.WebName }


                };
            }

            msg.Body = CreateBodyPart();

            return msg;
        }

        /// <summary>
        /// Render  <paramref name="layout"/> and add the addresses to <paramref name="mailAddressCollection"/>
        /// </summary>
        /// <param name="mailAddressCollection">Addresses appended to this list</param>
        /// <param name="layout">layout with addresses, ; separated</param>
        /// <param name="logEvent">event for rendering the <paramref name="layout"/></param>
        /// <returns>added a address?</returns>
        private static bool AddAddresses(InternetAddressList mailAddressCollection, Layout layout, LogEventInfo logEvent)
        {
            var added = false;
            if (layout != null)
            {
                foreach (string mail in layout.Render(logEvent).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailAddressCollection.Add(MailboxAddress.Parse(mail));
                    added = true;
                }
            }

            return added;
        }
    }
}

