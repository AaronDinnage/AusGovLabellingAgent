using Microsoft.Exchange.Data.Mime;
using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Email;
using Microsoft.Exchange.Data.Transport.Routing;
using System;
using System.Text.RegularExpressions;

namespace AusGovLabellingAgent
{
    public static class Common
    {
        public static void ProcessOutboundMessage(SmtpServer server, QueuedMessageEventSource source, EmailMessage message)
        {
            Logging.Log.Information("Common.ProcessOutboundMessage()");
            Logging.Log.Information("Message ID: {0}", message.MessageId);

            var headers = message.MimeDocument.RootPart.Headers;

            // Process Shortcut ...
            if (Config.Current.EnableShortcuts)
            {
                var classification = GetShortcutClassification(message.Subject);
                if (classification != null)
                {
                    // Trim the shortcut off the subject
                    string shortcut = classification.Shortcut;
                    string subject = message.Subject.TrimEnd();
                    subject = subject.Substring(0, subject.Length - shortcut.Length);
                    message.Subject = subject;

                    if (classification.Marking != null)
                    {
                        var protectiveMarking = classification.Marking.Clone();
                        protectiveMarking.Origin = message.Sender.SmtpAddress;
                        protectiveMarking.Method = Config.Current.MsipMethodManual;
                        protectiveMarking.SetDate = DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat);
                        // NOTE: DateTime.UtcNow ? or message.Date?

                        // Apply markings ...
                        ApplySubjectMarking(message, protectiveMarking);
                        ApplyHeaderMarking(headers, protectiveMarking);
                        ApplyMsipMarking(headers, protectiveMarking, classification.MsipLabel);
                    }

                    return;
                }
            }

            // Get MSIP Marking
            var msipHeader = headers.FindFirst(Config.Current.MsipHeaderName);
            if (msipHeader != null)
            {
                string headerValue = msipHeader.Value;
                var classification = GetMsipClassification(headerValue);

                if (classification != null)
                {
                    if (!string.Equals(classification.Key, Config.Current.MsipBypassLabel, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var protectiveMarking = GetMsipMarking(headerValue, classification);
                        ApplySubjectMarking(message, protectiveMarking);
                        ApplyHeaderMarking(headers, protectiveMarking);
                    }

                    return;
                }
                else
                {
                    Logging.Log.Warning("Unrecognized MSIP header: {0}", headerValue);
                }
            }

            // Apply mandatory labels for specific senders ...
            if (Config.Current.MandatoryClassifications != null && Config.Current.MandatoryClassifications.Length > 0)
            {
                var force = Array.Find(Config.Current.MandatoryClassifications,
                    x => string.Equals(message.Sender.SmtpAddress, x.Key, StringComparison.CurrentCultureIgnoreCase));

                if (force != null)
                {
                    var classification = Array.Find(Config.Current.Classifications,
                        x => string.Equals(force.Value, x.Key, StringComparison.CurrentCultureIgnoreCase));

                    if (classification != null)
                    {
                        // Accommodate bypass labels by not failing on .Marking == null
                        if (classification.Marking != null)
                        {
                            var protectiveMarking = classification.Marking.Clone();
                            protectiveMarking.Origin = message.Sender.SmtpAddress;
                            protectiveMarking.Method = Config.Current.MsipMethodMandatory;
                            protectiveMarking.SetDate = DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat);

                            // Apply markings ...
                            ApplySubjectMarking(message, protectiveMarking);
                            ApplyHeaderMarking(headers, protectiveMarking);
                            ApplyMsipMarking(headers, protectiveMarking, classification.MsipLabel);
                        }

                        return;
                    }
                    else
                    {
                        Logging.Log.Error("Mandatory classification sender is missing classification: {0}, {1}",
                            message.Sender.SmtpAddress,
                            force.Value);
                    }
                }
            }

            // Apply a Default Label ...
            if (!string.IsNullOrWhiteSpace(Config.Current.DefaultOutboundLabel))
            {
                ApplyDefaultLabel(message, headers, Config.Current.DefaultOutboundLabel);
                return;
            }

            // Bounce unlabelled emails ...
            if (Config.Current.BounceUnlabelledEmails)
            {
                BounceEmail(server, source, message);
                return;
            }
        }
        public static void ProcessInboundMessage(EmailMessage message)
        {
            Logging.Log.Information("Common.ProcessInboundMessage()");
            Logging.Log.Information("Message ID: {0}", message.MessageId);

            var headers = message.MimeDocument.RootPart.Headers;

            // Header Label ...
            var xHeader = headers.FindFirst(Config.Current.HeaderName);
            if (xHeader != null)
            {
                string headerValue = xHeader.Value;

                var headerMarking = ProtectiveMarking.FromRegex(headerValue, Config.Current.HeaderRegex, Config.Current.RegexOptions);
                if (headerMarking != null)
                {
                    var classification = Array.Find(Config.Current.Classifications,
                        x => ProtectiveMarking.Equals(x.Marking, headerMarking));

                    if (classification != null)
                    {
                        headerMarking.Method = Config.Current.MsipMethodAutomatic;
                        headerMarking.SetDate = DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat);

                        ApplyMsipMarking(headers, headerMarking, classification.MsipLabel);
                    }
                    else if (!string.IsNullOrWhiteSpace(Config.Current.MsipBypassLabel))
                    {
                        // TODO: Apply bypass label
                    }
                    else
                    {
                        // 
                    }
                }
                else
                {
                    Logging.Log.Warning("Unrecognized Header Label: {0}", headerValue);
                }

                return;
            }

            // Subject Label ...
            var subjectMarking = ProtectiveMarking.FromRegex(message.Subject, Config.Current.SubjectRegex, Config.Current.RegexOptions);
            if (subjectMarking != null)
            {
                var classification = Array.Find(Config.Current.Classifications,
                    x => ProtectiveMarking.Equals(x.Marking, subjectMarking));

                if (classification != null)
                {
                    subjectMarking.Method = Config.Current.MsipMethodAutomatic;
                    subjectMarking.SetDate = DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat);

                    ApplyMsipMarking(headers, subjectMarking, classification.MsipLabel);
                    return;
                }
                else if (!string.IsNullOrWhiteSpace(Config.Current.MsipBypassLabel))
                {
                    // TODO: Apply bypass label
                }
                else
                {
                    // 
                }

                return;
            }

            // Default inbound label ...
            if (!string.IsNullOrWhiteSpace(Config.Current.DefaultInboundLabel))
            {
                ApplyDefaultLabel(message, headers, Config.Current.DefaultInboundLabel, false, false, true);
                return;
            }
        }

        public static bool ApplyDefaultLabel(EmailMessage message, HeaderList headers, string key, bool subject = true, bool header = true, bool msip = true)
        {
            Logging.Log.Debug("Common.ApplyDefaultLabel()");

            var classification = Array.Find(Config.Current.Classifications,
                x => string.Equals(x.Key, key, StringComparison.CurrentCultureIgnoreCase));

            if (classification != null)
            {
                // NOTE: What is the purpose of a bypass in a default scenario?
                if (classification.Marking != null) // Accommodate Bypass labels by not failing if .Marking == null
                {
                    var protectiveMarking = classification.Marking.Clone();
                    protectiveMarking.Origin = message.Sender.SmtpAddress;
                    protectiveMarking.Method = Config.Current.MsipMethodDefault;
                    protectiveMarking.SetDate = DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat);

                    // Apply markings ...
                    if (subject)    ApplySubjectMarking(message, protectiveMarking);
                    if (header)     ApplyHeaderMarking(headers, protectiveMarking);
                    if (msip)       ApplyMsipMarking(headers, protectiveMarking, classification.MsipLabel);
                }
                else
                {
                    Logging.Log.Error("Default label definition not found: {0}", key);
                }

                // TODO: Shouldn't a bypass still apply the MSIP header?

                return true;
            }
            else
            {
                Logging.Log.Error("Default label definition not found: {0}", key);

                // Can't find Default Label Definition
                return false;
            }
        }
        public static void ApplyMsipMarking(HeaderList headers, ProtectiveMarking protectiveMarking, string msipLabel)
        {
            Logging.Log.Debug("Common.ApplyMsipMarking()");

            msipLabel = msipLabel.Replace(Config.Current.MsipMethodTag, protectiveMarking.Method);
            msipLabel = msipLabel.Replace(Config.Current.MsipOwnerTag, protectiveMarking.Origin);
            msipLabel = msipLabel.Replace(Config.Current.MsipSetDateTag, protectiveMarking.SetDate);

            ReplaceOrSetHeader(headers, Config.Current.MsipHeaderName, msipLabel);
        }
        public static void ApplyHeaderMarking(HeaderList headers, ProtectiveMarking protectiveMarking)
        {
            Logging.Log.Debug("Common.ApplyHeaderMarking()");

            string headerName = Config.Current.HeaderName;
            string headerValue = protectiveMarking.Header();

            ReplaceOrSetHeader(headers, headerName, headerValue);
        }
        public static void ReplaceOrSetHeader(HeaderList headers, string headerName, string headerValue)
        {
            Logging.Log.Debug("Common.ReplaceOrSetHeader()");

            var header = headers.FindFirst(headerName);
            if (header != null)
            {
                header.Value = headerValue;
            }
            else
            {
                header = new TextHeader(headerName, headerValue);
                headers.InsertBefore(header, headers.LastChild);
            }
        }
        public static void ApplySubjectMarking(EmailMessage message, ProtectiveMarking protectiveMarking)
        {
            Logging.Log.Debug("Common.ApplySubjectMarking()");

            // Wipe current marking(s)
            message.Subject = Regex.Replace(message.Subject, Config.Current.SubjectRegex, string.Empty, Config.Current.RegexOptions);

            // Append new marking
            message.Subject += " " + protectiveMarking.Subject();
        }

        public static Classification GetMsipClassification(string msipHeader)
        {
            Logging.Log.Debug("Common.GetMsipClassification()");

            var classification = Array.Find(Config.Current.Classifications,
                x => msipHeader.EndsWith("=" + x.Key, StringComparison.CurrentCultureIgnoreCase));

            return classification;
        }
        public static ProtectiveMarking GetMsipMarking(string msipHeader, Classification classification)
        {
            Logging.Log.Debug("Common.GetMsipMarking()");

            var protectiveMarking = classification.Marking.Clone();

            var ownerMatch = Regex.Match(msipHeader, Config.Current.MsipRegexOwner, Config.Current.RegexOptions);
            if (ownerMatch != null)
            {
                var owner = ownerMatch.Groups["value"].Value;
                protectiveMarking.Origin = owner;
            }

            var setDateMatch = Regex.Match(msipHeader, Config.Current.MsipRegexSetDate, Config.Current.RegexOptions);
            if (setDateMatch != null)
            {
                var setDate = setDateMatch.Groups["value"].Value;
                protectiveMarking.SetDate = setDate;
            }

            var methodMatch = Regex.Match(msipHeader, Config.Current.MsipRegexMethod, Config.Current.RegexOptions);
            if (methodMatch != null)
            {
                var method = methodMatch.Groups["value"].Value;
                protectiveMarking.Method = method;
            }

            return protectiveMarking;
        }
        public static Classification GetShortcutClassification(string subject)
        {
            Logging.Log.Debug("Common.GetShortcutClassification()");

            subject = subject.TrimEnd();

            var classification = Array.Find(Config.Current.Classifications,
                x => !string.IsNullOrWhiteSpace(x.Shortcut) &&
                    subject.EndsWith(x.Shortcut, StringComparison.CurrentCultureIgnoreCase));

            return classification;
        }

        private static void BounceEmail(SmtpServer server, QueuedMessageEventSource source, EmailMessage message)
        {
            Logging.Log.Information("Common.BounceEmail()");

            var ndrMessage = EmailMessage.Create();
            ndrMessage.From = new EmailRecipient(Config.Current.NdrDisplayName, Config.Current.NdrEmailAddress);
            ndrMessage.To.Add(new EmailRecipient(message.Sender.DisplayName, message.Sender.SmtpAddress));
            ndrMessage.Subject = Config.Current.NdrSubject;

            // Attach the original message
            if (Config.Current.NdrIncludeAttachment)
            {
                var attach = ndrMessage.Attachments.Add("RejectedMessage", "message/rfc822");
                attach.EmbeddedMessage = message;
            }

            // Submit the ndr for delivery
            server.SubmitMessage(ndrMessage);

            // Cancel the original message
            source.Delete();
        }
    }
}
