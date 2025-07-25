﻿// 
// Copyright (c) 2004-2022 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Linq;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.MailKit
{
    /// <summary>
    /// Sends log messages by email using SMTP protocol.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Mail-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Mail-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Simple/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Simple/Example.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple log messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Buffered/NLog.config" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Buffered/Example.cs" />
    /// </example>
    [Target("Mail")]
    [Target("MailKit")]
    public class MailTarget : TargetWithLayoutHeaderAndFooter
    {
        private static readonly Encoding DefaultEncoding = System.Text.Encoding.UTF8;
        private const SecureSocketOptions DefaultSecureSocketOption = SecureSocketOptions.StartTlsWhenAvailable;

        private const string RequiredPropertyIsEmptyFormat = "After the processing of the MailTarget's '{0}' property it appears to be empty. The email message will not be sent.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the Body-Layout is: <code>${message}${newline}</code>
        /// </remarks>
        public MailTarget()
        {
            Body = "${message}${newline}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the Body-Layout is: <code>${message}${newline}</code>
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
        public Layout From { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets recipients' email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='11' />
        public Layout To { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets CC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='12' />
        public Layout? Cc { get; set; }

        /// <summary>
        /// Gets or sets BCC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='13' />
        public Layout? Bcc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add new lines between log entries.
        /// </summary>
        /// <value>A value of <c>true</c> if new lines should be added; otherwise, <c>false</c>.</value>
        /// <docgen category='Message Options' order='99' />
        public bool AddNewLines { get; set; }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        /// <docgen category='Message Options' order='5' />
        public Layout Subject { get; set; } = "Message from NLog on ${machinename}";

        /// <summary>
        /// Gets or sets mail message body (repeated for each log message send in one mail).
        /// </summary>
        /// <remarks>Alias for the <c>Layout</c> property.</remarks>
        /// <docgen category='Message Options' order='6' />
        public Layout Body
        {
            get => Layout;
            set => Layout = value;
        }

        /// <summary>
        /// Gets or sets encoding to be used for sending e-mail.
        /// </summary>
        /// <docgen category='Message Options' order='20' />
        public Layout<Encoding> Encoding { get; set; } = DefaultEncoding;

        /// <summary>
        /// Gets or sets a value indicating whether to send message as HTML instead of plain text.
        /// </summary>
        /// <docgen category='Message Options' order='11' />
        public Layout<bool> Html { get; set; } = false;

        /// <summary>
        /// Gets or sets SMTP Server to be used for sending.
        /// </summary>
        /// <docgen category='SMTP Options' order='10' />
        public Layout SmtpServer { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets SMTP Authentication mode.
        /// </summary>
        /// <docgen category='SMTP Options' order='11' />
        public Layout<SmtpAuthenticationMode> SmtpAuthentication { get; set; } = SmtpAuthenticationMode.None;

        /// <summary>
        /// Gets or sets the username used to connect to SMTP server (used when <see cref="SmtpAuthentication"/> is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='12' />
        public Layout? SmtpUserName { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate against SMTP server (used when  <see cref="SmtpAuthentication"/> is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='13' />
        public Layout? SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL (secure sockets layer) should be used when communicating with SMTP server.
        /// 
        /// See also <see cref="SecureSocketOption" />
        /// </summary>
        /// <docgen category='SMTP Options' order='14' />.
        public Layout<bool> EnableSsl { get; set; } = false;

        /// <summary>
        /// Get or set whether the client should use the REQUIRETLS extension if it is available.
        /// </summary>
        /// <remarks>
        /// <para>Gets or sets whether the client should use the REQUIRETLS extension if it is available.</para>
        /// <para>The REQUIRETLS extension (as defined in rfc8689) is a way to ensure that every SMTP server
        /// that a message passes through on its way to the recipient is required to use a TLS connection in
        /// order to transfer the message to the next SMTP server.</para>
        /// <note type="note">This feature is only available if connected SMTP server supports capability
        /// <see cref="SmtpCapabilities.RequireTLS"/> flag when sending the message.</note>
        /// </remarks>
        public Layout<bool> RequireTLS { get; set; } = false;

        /// <summary>
        /// Provides a way of specifying the SSL and/or TLS encryption
        /// 
        /// If <see cref="EnableSsl" /> is <c>true</c>, then <see cref="SecureSocketOptions.SslOnConnect" /> will be used.
        /// </summary>
        [DefaultValue(DefaultSecureSocketOption)]
        [CLSCompliant(false)]
        public Layout<SecureSocketOptions> SecureSocketOption { get; set; } = DefaultSecureSocketOption;

        /// <summary>
        /// Gets or sets the port number that SMTP Server is listening on.
        /// </summary>
        /// <docgen category='SMTP Options' order='15' />
        public Layout<int> SmtpPort { get; set; } = 25;

        /// <summary>
        /// Gets or sets a value indicating whether SmtpClient should ignore invalid certificate.
        /// </summary>
        /// <docgen category='SMTP Options' order='16' />
        public Layout<bool> SkipCertificateValidation { get; set; } = false;

        /// <summary>
        /// Gets or sets the priority used for sending mails.
        /// </summary>
        /// <docgen category='Message Options' order='100' />
        public Layout Priority { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether NewLine characters in the body should be replaced with <br/> tags.
        /// </summary>
        /// <remarks>Only happens when <see cref="Html"/> is set to true.</remarks>
        public Layout<bool> ReplaceNewlineWithBrTagInHtml { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating the SMTP client timeout (in milliseconds)
        /// </summary>
        /// <remarks>Warning: zero is not infinite waiting</remarks>
        public Layout<int> Timeout { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the folder where applications save mail messages to be processed by the local SMTP server.
        /// </summary>
        /// <docgen category='SMTP Options' order='17' />
        public Layout? PickupDirectoryLocation { get; set; }

        /// <summary>
        /// Gets the array of email headers that are transmitted with this email message
        /// </summary>
        /// <docgen category='Message Options' order='100' />
        [ArrayParameter(typeof(MethodCallParameter), "mailheader")]
        public IList<MethodCallParameter> MailHeaders { get; } = new List<MethodCallParameter>();

        /// <inheritdoc/>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write(new[] { logEvent });
        }

        /// <inheritdoc/>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 1)
            {
                ProcessSingleMailMessage(logEvents);
            }
            else
            {
                var buckets = logEvents.GroupBy(l => GetSmtpSettingsKey(l.LogEvent));
                foreach (var bucket in buckets)
                {
                    var eventInfos = bucket;
                    ProcessSingleMailMessage(eventInfos);
                }
            }
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            InternalLogger.Debug("Init mailtarget with mailkit");
            CheckRequiredParameters();

            base.InitializeTarget();
        }

        /// <summary>
        /// Create mail and send with SMTP
        /// </summary>
        /// <param name="events">event printed in the body of the event</param>
        private void ProcessSingleMailMessage(IEnumerable<AsyncLogEventInfo> events)
        {
            try
            {
                LogEventInfo firstEvent = events.FirstOrDefault().LogEvent;
                LogEventInfo lastEvent = events.LastOrDefault().LogEvent;
                if (firstEvent is null || lastEvent is null)
                {
                    throw new NLogRuntimeException("We need at least one event.");
                }

                // unbuffered case, create a local buffer, append header, body and footer
                var bodyBuffer = CreateBodyBuffer(events, firstEvent, lastEvent);

                var message = CreateMailMessage(lastEvent, bodyBuffer.ToString());

                var pickupFolder = RenderLogEvent(PickupDirectoryLocation, lastEvent);
                if (!string.IsNullOrEmpty(pickupFolder))
                {
                    var emailFilePath = ResolvePickupDirectoryLocationFilePath(pickupFolder);
                    message.WriteTo(emailFilePath);
                }
                else
                {
                    SendMailMessage(message, lastEvent);
                }

                foreach (var ev in events)
                {
                    ev.Continuation(null);
                }
            }
            catch (Exception exception)
            {
                //always log
                InternalLogger.Error(exception, "{0}: Error sending mail.", this);

                if (LogManager.ThrowExceptions)
                    throw;

                foreach (var ev in events)
                {
                    ev.Continuation(exception);
                }
            }
        }

        private void SendMailMessage(MimeMessage message, LogEventInfo lastEvent)
        {
            using (var client = new SmtpClient())
            {
                client.Timeout = RenderLogEvent(Timeout, lastEvent);

                var renderedHost = RenderLogEvent(SmtpServer, lastEvent);
                if (string.IsNullOrEmpty(renderedHost))
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, nameof(SmtpServer)));
                }

                var enableSsl = RenderLogEvent(EnableSsl, lastEvent);
                var requireTLS = RenderLogEvent(RequireTLS, lastEvent);

                var secureSocketOptions = enableSsl ? SecureSocketOptions.SslOnConnect : RenderLogEvent(SecureSocketOption, lastEvent, DefaultSecureSocketOption);
                var smtpPort = RenderLogEvent(SmtpPort, lastEvent);
                InternalLogger.Debug("Sending mail to {0} using {1}:{2}", message.To, renderedHost, smtpPort);
                InternalLogger.Trace("  Subject: '{0}'", message.Subject);
                InternalLogger.Trace("  From: '{0}'", message.From);

                var skipCertificateValidation = RenderLogEvent(SkipCertificateValidation, lastEvent);
                if (skipCertificateValidation)
                {
                    client.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                }

                if (requireTLS)
                {
                    client.RequireTLS = true;   // Requires SMTP Server capability SmtpCapabilities.RequireTLS
                }

                client.Connect(renderedHost, smtpPort, secureSocketOptions);
                InternalLogger.Trace("{0}: Connecting succesfull with SmtpCapabilities={1}", this, client.Capabilities);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication

                var smtpAuthentication = RenderLogEvent(SmtpAuthentication, lastEvent);
                if (smtpAuthentication == SmtpAuthenticationMode.Basic)
                {
                    var userName = RenderLogEvent(SmtpUserName, lastEvent);
                    var password = RenderLogEvent(SmtpPassword, lastEvent);

                    InternalLogger.Trace("{0}: Authenticate with username '{1}'", this, userName);
                    client.Authenticate(userName, password);
                }

                client.Send(message);
                InternalLogger.Trace("{0}: Sending mail done. Disconnecting", this);
                client.Disconnect(true);
                InternalLogger.Trace("{0}: Disconnected", this);
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
            var addNewLines = RenderLogEvent(AddNewLines, firstEvent, false);
            if (Header != null)
            {
                bodyBuffer.Append(RenderLogEvent(Header, firstEvent));
                if (addNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }

            foreach (var eventInfo in events)
            {
                bodyBuffer.Append(RenderLogEvent(Layout, eventInfo.LogEvent));
                if (addNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }

            if (Footer != null)
            {
                bodyBuffer.Append(RenderLogEvent(Footer, lastEvent));
                if (addNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }
            return bodyBuffer;
        }

        /// <summary>
        /// Handle <paramref name="pickupDirectoryLocation"/> if it is a virtual directory.
        /// </summary>
        internal string ResolvePickupDirectoryLocationFilePath(string pickupDirectoryLocation)
        {
            const string virtualPathPrefix = "~/";

            if (pickupDirectoryLocation.StartsWith(virtualPathPrefix, StringComparison.Ordinal))
            {
                // Support for Virtual Paths
                var root = AppDomain.CurrentDomain.BaseDirectory;
                var directory = pickupDirectoryLocation.Substring(virtualPathPrefix.Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
                pickupDirectoryLocation = System.IO.Path.Combine(root, directory);
            }

            string filename;
            string pathAndFilename;
            while (true)
            {
                filename = Guid.NewGuid().ToString() + ".eml";
                pathAndFilename = System.IO.Path.Combine(pickupDirectoryLocation, filename);
                if (!System.IO.File.Exists(pathAndFilename))
                    break;
            }

            InternalLogger.Debug("{0}: Writing mail to file: {1}", this, pathAndFilename);
            return pathAndFilename;
        }

        private void CheckRequiredParameters()
        {
            if (From is null || ReferenceEquals(From, Layout.Empty))
            {
                throw new NLogConfigurationException("MailTarget - From address is required");
            }

            if (To is null || ReferenceEquals(To, Layout.Empty))
            {
                throw new NLogConfigurationException("MailTarget - To address is required");
            }

            var smtpAuthentication = RenderLogEvent(SmtpAuthentication, LogEventInfo.CreateNullEvent());
            if (smtpAuthentication == SmtpAuthenticationMode.Ntlm)
            {
                throw new NLogConfigurationException("MailTarget - SmtpAuthentication NTLM not yet supported");
            }

            if ((PickupDirectoryLocation is null || ReferenceEquals(PickupDirectoryLocation, Layout.Empty)) && (SmtpServer is null || ReferenceEquals(SmtpServer, Layout.Empty)))
            {
                throw new NLogConfigurationException("MailTarget - SmtpServer is required");
            }
        }

        /// <summary>
        /// Create key for grouping. Needed for multiple events in one mail message
        /// </summary>
        /// <param name="logEvent">event for rendering layouts   </param>  
        /// <returns>string to group on</returns>
        private string GetSmtpSettingsKey(LogEventInfo logEvent)
        {
            return $@"{RenderLogEvent(From, logEvent)}
{RenderLogEvent(To, logEvent)}
{RenderLogEvent(Cc, logEvent)}
{RenderLogEvent(Bcc, logEvent)}
{RenderLogEvent(SmtpServer, logEvent)}
{RenderLogEvent(SmtpPassword, logEvent)}
{RenderLogEvent(SmtpUserName, logEvent)}";
        }

        /// <summary>
        /// Create the mail message with the addresses, properties and body.
        /// </summary>
        private MimeMessage CreateMailMessage(LogEventInfo lastEvent, string body)
        {
            var msg = new MimeMessage();

            var renderedFrom = RenderLogEvent(From, lastEvent);

            if (string.IsNullOrEmpty(renderedFrom))
            {
                throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "From"));
            }
            msg.From.Add(MailboxAddress.Parse(renderedFrom));

            var addedTo = AddAddresses(msg.To, To, lastEvent);
            var addedCc = AddAddresses(msg.Cc, Cc, lastEvent);
            var addedBcc = AddAddresses(msg.Bcc, Bcc, lastEvent);

            if (!addedTo && !addedCc && !addedBcc)
            {
                throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "To/Cc/Bcc"));
            }

            msg.Subject = (RenderLogEvent(Subject, lastEvent) ?? string.Empty).Trim();

            var renderedPriority = RenderLogEvent(Priority, lastEvent);
            if (!string.IsNullOrEmpty(renderedPriority))
            {
                msg.Priority = ParseMessagePriority(renderedPriority);
            }

            var newBody = body;
            var html = RenderLogEvent(Html, lastEvent);
            var replaceNewlineWithBrTagInHtml = RenderLogEvent(ReplaceNewlineWithBrTagInHtml, lastEvent);
            if (html && replaceNewlineWithBrTagInHtml)
            {
                newBody = newBody?.Replace(Environment.NewLine, "<br/>");
                if (newBody?.IndexOf('\n') >= 0)
                {
                    newBody = newBody.Replace("\n", "<br/>");
                }
            }

            var encoding = RenderLogEvent(Encoding, lastEvent, DefaultEncoding);
            msg.Body = new TextPart(html ? TextFormat.Html : TextFormat.Plain)
            {
                Text = newBody,
                ContentType = { Charset = encoding?.WebName }
            };

            if (MailHeaders?.Count > 0)
            {
                for (int i = 0; i < MailHeaders.Count; i++)
                {
                    var headerName = MailHeaders[i].Name;
                    if (string.IsNullOrEmpty(headerName))
                        continue;

                    string headerValue = RenderLogEvent(MailHeaders[i].Layout, lastEvent);
                    if (headerValue is null)
                        continue;

                    msg.Headers.Add(headerName, headerValue);
                }
            }

            return msg;
        }

        internal static MessagePriority ParseMessagePriority(string priority)
        {
            if (string.IsNullOrWhiteSpace(priority))
            {
                return MessagePriority.Normal;
            }

            priority = priority.Trim();
            if (priority.Equals("High", StringComparison.OrdinalIgnoreCase))
            {
                return MessagePriority.Urgent;
            }

            if (priority.Equals("Low", StringComparison.OrdinalIgnoreCase))
            {
                return MessagePriority.NonUrgent;
            }

            MessagePriority messagePriority;
            try
            {
                messagePriority = (MessagePriority)Enum.Parse(typeof(MessagePriority), priority, true);
            }
            catch
            {
                InternalLogger.Warn("Could not convert '{0}' to MessagePriority, valid values are NonUrgent, Normal and Urgent. " +
                                    "Also High and Low could be used as alternatives to Urgent and NonUrgent. " +
                                    "Using normal priority as fallback.");
                messagePriority = MessagePriority.Normal;
            }

            return messagePriority;
        }

        /// <summary>
        /// Render  <paramref name="layout" /> and add the addresses to <paramref name="mailAddressCollection" />
        /// </summary>
        /// <param name="mailAddressCollection">Addresses appended to this list</param>
        /// <param name="layout">layout with addresses, ; separated</param>
        /// <param name="logEvent">event for rendering the <paramref name="layout" /></param>
        /// <returns>added a address?</returns>
        private bool AddAddresses(InternetAddressList mailAddressCollection, Layout? layout, LogEventInfo logEvent)
        {
            var added = false;
            var mailAddresses = RenderLogEvent(layout, logEvent);

            if (!string.IsNullOrEmpty(mailAddresses))
            {
                foreach (string mail in mailAddresses.Split(';'))
                {
                    var mailAddress = mail.Trim();
                    if (string.IsNullOrEmpty(mailAddress))
                        continue;

                    mailAddressCollection.Add(MailboxAddress.Parse(mail));
                    added = true;
                }
            }

            return added;
        }
    }
}