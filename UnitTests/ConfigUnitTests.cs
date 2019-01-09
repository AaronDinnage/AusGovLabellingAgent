using AusGovLabellingAgent;
using AusGovLabellingAgent.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnitTests
{
    [TestClass]
    public class ConfigUnitTests
    {
        [TestMethod]
        public void Save()
        {
            Logging.Log.Debug("ConfigUnitTests.Save()");

            var assembly = Assembly.GetExecutingAssembly();
            string configurationDirectory = Path.GetDirectoryName(assembly.Location);

            string configFilePath = Path.Combine(configurationDirectory, Config.Filename);

            BuildDefaultConfig();

            Config.Save();
        }

        private void BuildDefaultConfig()
        {
            Logging.Log.Debug("ConfigUnitTests.BuildDefaultConfig()");

            Config.Current = new Config
            {
                AutoReload = true,
                AuditLevel = LogLevel.All,

                HeaderName = "X-Protective-Marking",

                MsipHeaderName = "msip_labels",
                //MsipHeaderKeyRegex = "_Name={0};$",
                MsipOwnerTag = "%%Owner%%",
                MsipSetDateTag = "%%SetDate%%",
                MsipMethodTag = "%%Method%%",
                MsipSetDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'",
                MsipRegexOwner = @".*(?:_Owner=(?<value>[^;]+);).*",
                MsipRegexSetDate = @".*(?:_SetDate=(?<value>[^;]+);).*",
                MsipRegexMethod = @".*(?:_Method=(?<value>[^;]+);).*",
                MsipBypassLabel = "Bypass",
                MsipMethodDefault = "Default",
                MsipMethodManual = "Manual",
                MsipMethodAutomatic = "Automatic",
                MsipMethodMandatory = "Mandatory",
                // Note: Recommended method is not accommodated.

                NdrDisplayName = "Mail Server",
                NdrEmailAddress = "reject@contoso.com",
                NdrSubject = "Message Rejected - Missing Valid Classification [SEC=OFFICIAL]",
                NdrIncludeAttachment = true,

                SubjectRegex = @"\s*\[(?:SEC=(?<sec>[^,\]]+)|DLM=(?<dlm>[^,\]]+)|SEC=(?<sec>[^,\]]+),\s*DLM=(?<dlm>[^,\]]+))(?:,\s*CAVEAT=(?<caveat>[^,\]]+))*(?:,\s*EXPIRES=(?<expires>[^,\]]+),\s*DOWNTO=(?<downTo>[^,\]]+))?(?:,\s*ACCESS=(?<access>[^,\]]+))*]\s*",
                HeaderRegex = @"\s*(?:VER=(?<ver>[^,]+))(?:,\s*NS=(?<ns>[^,]+))(?:,\s*SEC=(?<sec>[^,]+))?(?:,\s*DLM=(?<dlm>[^,]+))?(?:,\s*CAVEAT=(?<caveat>[^,]+))*(?:,\s*EXPIRES=(?<expires>[^,]+),\s*DOWNTO=(?<downTo>[^,]+))?(?:,\s*ACCESS=(?<access>[^,]+))*(?:,\s*NOTE=(?<note>[^,]+))?(?:,\s*ORIGIN=(?<origin>[^,]+))?\s*",

                RegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,

                EnableShortcuts = true,
                DefaultOutboundLabel = "OFFICIAL",
                DefaultInboundLabel = "Bypass",

                BounceUnlabelledEmails = false,

                Classifications = new Classification[]
                {
                    new Classification ("Bypass", "#Bypass", null, // No Protective Marking
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=Bypass;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 Sensitivity=Bypass"),

                    new Classification("UNOFFICIAL", "#UF",
                        new ProtectiveMarking("2018.1", "gov.au", "UNOFFICIAL", null, null, null, null, null, null, null),
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=UNOFFICIAL;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 Sensitivity=UNOFFICIAL"),
                    new Classification("OFFICIAL", "#OF",
                        new ProtectiveMarking("2018.1", "gov.au", "OFFICIAL", null, null, null, null, null, null, null),
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=OFFICIAL;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 Sensitivity=OFFICIAL"),
                    new Classification("OFFICIAL Sensitive", "#OS",
                        new ProtectiveMarking("2018.1", "gov.au", "OFFICIAL:Sensitive", null, null, null, null, null, null, null),
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=OFFICIAL Sensitive;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 Sensitivity=OFFICIAL Sensitive"),
                    new Classification("PROTECTED", "#P",
                        new ProtectiveMarking("2018.1", "gov.au", "PROTECTED", null, null, null, null, null, null, null),
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SiteId=72f988bf-86f1-41af-91ab-2d7cd011db47;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Owner=%%Owner%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_SetDate=%%SetDate%%;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Name=PROTECTED;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Application=Microsoft Azure
 Information Protection;
 MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Extended_MSFT_Method=%%Method%%;
 Sensitivity=PROTECTED"),
                     new Classification("PROTECTED Cabinet", "#PC",
                        new ProtectiveMarking("2018.1", "gov.au", "PROTECTED", null, new string[] { "RI:CABINET" }, null, null, null, null, null),
@" MSIP_Label_074e257c-5848-4582-9a6f-34a182080e71_Enabled=True;
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
 Sensitivity=PROTECTED Cabinet"),
                },

                MandatoryClassifications = new StringPair[]
                {
                    new StringPair {Key = "mfd@contoso.com", Value = "PROTECTED" },
                    new StringPair {Key = "lob@contoso.com", Value = "OFFICIAL Sensitive" },
                },
            };
        }
    }
}
