using AusGovLabellingAgent;
using Microsoft.Exchange.Data.Transport.Email;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace UnitTests
{
    [TestClass()]
    public class CommonTests
    {
        const string MsipHeader = @" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=PROTECTED;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Enabled=True;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Owner=%%Owner%%;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_SetDate=%%SetDate%%;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Name=Cabinet;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Parent=074e257c-5848-4582-9a6f-34a182080e71;
 MSIP_Label_fec751d2-ff79-4176-b06f-8a50d675b290_Extended_MSFT_Method=%%Method%%;
 Sensitivity=PROTECTED Cabinet";
        const string Shortcut = "#PC";
        const string Key = "PROTECTED Cabinet";
        const string SecurityClassification = "PROTECTED";

        [TestMethod()]
        public void GetMsipClassificationTest()
        {
            Logging.Log.Debug("CommonTests.GetMsipClassificationTest()");

            var classification = Common.GetMsipClassification(MsipHeader);

            Assert.IsTrue(classification.Key == Key);
        }

        [TestMethod()]
        public void GetMsipMarkingTest()
        {
            Logging.Log.Debug("CommonTests.GetMsipMarkingTest()");

            var classification = Common.GetMsipClassification(MsipHeader);

            var marking = Common.GetMsipMarking(MsipHeader, classification);

            Assert.IsTrue(marking.SecurityClassification == SecurityClassification);
        }

        [TestMethod()]
        public void GetShortcutClassificationTest()
        {
            Logging.Log.Debug("CommonTests.GetShortcutClassificationTest()");

            string subject = "This is a subject " + Shortcut;

            var classification = Common.GetShortcutClassification(subject);

            Assert.IsTrue(classification.Shortcut == Shortcut);
        }

        [TestMethod()]
        public void ApplySubjectMarkingTest()
        {
            Logging.Log.Debug("CommonTests.ApplySubjectMarkingTest()");

            var classification = Common.GetMsipClassification(MsipHeader);

            var protectiveMarking = Common.GetMsipMarking(MsipHeader, classification);

            string subject = "This is a subject";
            string subjectSuffix = protectiveMarking.Subject();
            string expectedSubject = subject + " " + subjectSuffix;

            using (var message = EmailMessage.Create())
            {
                message.Subject = subject;
                Common.ApplySubjectMarking(message, protectiveMarking);

                Assert.IsTrue(message.Subject == expectedSubject);
            }
        }

        [TestMethod()]
        public void ReplaceOrSetHeaderTest()
        {
            Logging.Log.Debug("CommonTests.ReplaceOrSetHeaderTest()");

            var classification = Common.GetMsipClassification(MsipHeader);

            var protectiveMarking = Common.GetMsipMarking(MsipHeader, classification);

            string headerName = "X-Header";
            string headerValue = "This is a header";

            using (var message = EmailMessage.Create())
            {
                var headers = message.MimeDocument.RootPart.Headers;

                var found = headers.FindAll(headerName);
                Assert.IsTrue(found.Length == 0);

                Common.ReplaceOrSetHeader(headers, headerName, headerValue);

                found = headers.FindAll(headerName);
                Assert.IsTrue(found.Length == 1);
                Assert.IsTrue(headers.FindFirst(headerName).Value == headerValue);

                headerValue = "new value";

                Common.ReplaceOrSetHeader(headers, headerName, headerValue);

                found = headers.FindAll(headerName);
                Assert.IsTrue(found.Length == 1);
                Assert.IsTrue(headers.FindFirst(headerName).Value == headerValue);
            }
        }

        [TestMethod()]
        public void ApplyHeaderMarkingTest()
        {
            Logging.Log.Debug("CommonTests.ApplyHeaderMarkingTest()");

            var classification = Common.GetMsipClassification(MsipHeader);
            var protectiveMarking = Common.GetMsipMarking(MsipHeader, classification);
            string headerValue = protectiveMarking.Header();

            using (var message = EmailMessage.Create())
            {
                var headers = message.MimeDocument.RootPart.Headers;

                Common.ApplyHeaderMarking(headers, protectiveMarking);

                var found = headers.FindAll(Config.Current.HeaderName);
                Assert.IsTrue(found.Length == 1);
                Assert.IsTrue(headers.FindFirst(Config.Current.HeaderName).Value == headerValue);

                Common.ApplyHeaderMarking(headers, protectiveMarking);

                found = headers.FindAll(Config.Current.HeaderName);
                Assert.IsTrue(found.Length == 1);
                Assert.IsTrue(headers.FindFirst(Config.Current.HeaderName).Value == headerValue);
            }
        }

        [TestMethod()]
        public void ApplyDefaultLabelTest()
        {
            Logging.Log.Debug("CommonTests.ApplyDefaultLabelTest()");

            var classification = Common.GetMsipClassification(MsipHeader);

            string msipLabel = classification.MsipLabel;
            msipLabel = msipLabel.Replace(Config.Current.MsipMethodTag, Config.Current.MsipMethodDefault);
            msipLabel = msipLabel.Replace(Config.Current.MsipOwnerTag, "aaron.dinnage@microsoft.com");
            //msipLabel = msipLabel.Replace(Config.Current.MsipSetDateTag, DateTime.UtcNow.ToString(Config.Current.MsipSetDateFormat));

            var protectiveMarking = Common.GetMsipMarking(msipLabel, classification);
            string headerValue = protectiveMarking.Header();

            string subject = "This is a subject";
            string subjectSuffix = protectiveMarking.Subject();
            string expectedSubject = subject + " " + subjectSuffix;

            using (var message = EmailMessage.Create())
            {
                message.Sender = new EmailRecipient("Aaron Dinnage", "aaron.dinnage@microsoft.com");
                message.Subject = subject;

                var headers = message.MimeDocument.RootPart.Headers;

                var result = Common.ApplyDefaultLabel(message, headers, Key);

                var setDateMatch = Regex.Match(headers.FindFirst(Config.Current.MsipHeaderName).Value, Config.Current.MsipRegexSetDate, Config.Current.RegexOptions);
                var setDate = setDateMatch.Groups["value"].Value;
                msipLabel = msipLabel.Replace(Config.Current.MsipSetDateTag, setDate);

                // Header
                var foundHeader = headers.FindAll(Config.Current.HeaderName);
                Assert.IsTrue(foundHeader.Length == 1);
                Assert.IsTrue(headers.FindFirst(Config.Current.HeaderName).Value == headerValue);

                // Subject
                Assert.IsTrue(message.Subject == expectedSubject);

                // Msip
                var foundMsip = headers.FindAll(Config.Current.MsipHeaderName);
                Assert.IsTrue(foundMsip.Length == 1);
                var find = headers.FindFirst(Config.Current.MsipHeaderName);
                string found = find.Value;
                Assert.IsTrue(found == msipLabel);
            }
        }
    }
}