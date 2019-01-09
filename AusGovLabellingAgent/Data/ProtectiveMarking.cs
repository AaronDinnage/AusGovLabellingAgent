using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace AusGovLabellingAgent
{
    [XmlRoot]
    public class ProtectiveMarking
    {
        public const string DefaultVersion = "2018.1";
        public const string PreviousVersion = "2012.3";
        public const string DefaultNamespace = "gov.au";

        [XmlAttribute]
        public string Version { get; set; } = DefaultVersion;
        [XmlAttribute]
        public string Namespace { get; set; } = DefaultNamespace;

        // Protective Marking Attributes
        [XmlAttribute]
        public string SecurityClassification { get; set; } = null;
        [XmlElement]
        public string[] Caveats { get; set; } = null;
        [XmlAttribute]
        public string Expires { get; set; } = null;
        [XmlAttribute]
        public string DownTo { get; set; } = null;
        [XmlElement]
        public string[] InformationManagementMarkers { get; set; } = null;
        [XmlAttribute]
        public string Note { get; set; } = null;
        [XmlAttribute]
        public string Origin { get; set; } = null;

        // Deprecated Attributes (2012.3)
        [XmlAttribute]
        public string Dlm { get; set; } = null;

        // MSIP Attributes
        [XmlIgnore]
        public string SetDate { get; set; } = null;
        [XmlIgnore]
        public string Method { get; set; } = null;


        public ProtectiveMarking() { }

        public ProtectiveMarking(string ver, string ns, string sec, string dlm, string[] caveats, string expires, string downTo, string[] access, string note, string origin)
        {
            Version                         = ver;
            Namespace                       = ns;
            SecurityClassification          = sec;
            Dlm                             = dlm;
            Caveats                         = caveats;
            Expires                         = expires;
            DownTo                          = downTo;
            InformationManagementMarkers    = access;
            Note                            = note;
            Origin                          = origin;
        }

        public string Subject()
        {
            Logging.Log.Information("ProtectiveMarking.Subject()");

            var elements = new List<string>();

            if (!string.IsNullOrWhiteSpace(SecurityClassification))
                elements.Add(string.Format("SEC={0}", SecurityClassification));

            // NOTE: DLM is deprecated
            if (!string.IsNullOrWhiteSpace(Dlm) && Version == PreviousVersion)
                elements.Add(string.Format("DLM={0}", Dlm));

            if (Caveats != null)
                foreach (var caveat in Caveats)
                    elements.Add(string.Format("CAVEAT={0}", caveat));

            if (!string.IsNullOrWhiteSpace(Expires))
            {
                elements.Add(string.Format("EXPIRES={0}", Expires));
                elements.Add(string.Format("DOWNTO={0}", DownTo));
            }

            if (InformationManagementMarkers != null)
                foreach (var access in InformationManagementMarkers)
                    elements.Add(string.Format("ACCESS={0}", access));

            string text = "[" + string.Join(", ", elements) + "]";

            Logging.Log.Debug("ProtectiveMarking.Subject() - Result: {0}", text);

            return text;
        }

        public string Header()
        {
            Logging.Log.Debug("ProtectiveMarking.Header()");

            var elements = new List<string>();

            elements.Add(string.Format("VER={0}", Version));
            elements.Add(string.Format("NS={0}", Namespace));
            elements.Add(string.Format("SEC={0}", SecurityClassification));

            if (Caveats != null)
                foreach (var caveat in Caveats)
                    elements.Add(string.Format("CAVEAT={0}", caveat));

            if (!string.IsNullOrEmpty(Expires))
            {
                elements.Add(string.Format("EXPIRES={0}", Expires));
                elements.Add(string.Format("DOWNTO={0}", DownTo));
            }

            if (InformationManagementMarkers != null)
                foreach (var access in InformationManagementMarkers)
                    elements.Add(string.Format("ACCESS={0}", access));

            if (!string.IsNullOrEmpty(Note))
                elements.Add(string.Format("NOTE={0}", Note));

            if (!string.IsNullOrEmpty(Origin))
                elements.Add(string.Format("ORIGIN={0}", Origin));

            string text = string.Join(", ", elements);

            Logging.Log.Debug("ProtectiveMarking.Header() - Result: {0}", text);

            return text;
        }

        public ProtectiveMarking Clone()
        {
            Logging.Log.Debug("ProtectiveMarking.Clone()");

            var protectiveMarking = new ProtectiveMarking()
            {
                // Current
                Version                         = Version,
                Namespace                       = Namespace,
                SecurityClassification          = SecurityClassification,
                Caveats                         = Caveats == null ? null : (string[])Caveats.Clone(),
                Expires                         = Expires,
                DownTo                          = DownTo,
                InformationManagementMarkers    = InformationManagementMarkers == null ? null : (string[])InformationManagementMarkers.Clone(),
                Note                            = Note,
                Origin                          = Origin,
                // Deprecated
                Dlm                             = Dlm,
                // Msip
                SetDate                         = SetDate,
                Method                          = Method,
            };

            return protectiveMarking;
        }

        public static ProtectiveMarking FromRegex(string text, string regex, RegexOptions regexOptions)
        {
            Logging.Log.Debug("ProtectiveMarking.FromRegex() - Text={0}", text);

            var match = Regex.Match(text, regex, regexOptions);
            if (!match.Success)
            {
                Logging.Log.Information("ProtectiveMarking.FromRegex() - No match");
                return null;
            }

            if (match.Captures.Count > 1)
                Logging.Log.Warning("ProtectiveMarking.FromRegex() - More than one label detected, processing first match");

            return FromRegexMatch(match);
        }

        public static ProtectiveMarking FromRegexMatch(Match match)
        {
            Logging.Log.Debug("ProtectiveMarking.FromRegexMatch()");

            List<string> items;

            var marking = new ProtectiveMarking();

            marking.Version = match.Groups["ver"].Value;
            marking.Namespace = match.Groups["ns"].Value;
            marking.SecurityClassification = match.Groups["sec"].Value;

            var caveats = match.Groups["caveat"].Captures;
            items = new List<string>();
            foreach (Group item in caveats)
                items.Add(item.Value);
            if (items.Count > 0)
                marking.Caveats = items.ToArray();

            marking.Expires = match.Groups["expires"].Value;
            marking.DownTo = match.Groups["downTo"].Value;

            var informationManagementMarkers = match.Groups["access"].Captures;
            items = new List<string>();
            foreach (Group item in informationManagementMarkers)
                items.Add(item.Value);
            if (items.Count > 0)
                marking.InformationManagementMarkers = items.ToArray();

            marking.Note = match.Groups["note"].Value;
            marking.Origin = match.Groups["origin"].Value;

            // Deprecated (2012.3)
            marking.Dlm = match.Groups["dlm"].Value;

            return marking;
        }

        public static bool Equals(ProtectiveMarking x, ProtectiveMarking y)
        {
            Logging.Log.Debug("ProtectiveMarking.Equals()");

            // Compares only the things that matter, ignores Notes & Origin (and MSIP attributes) ...

            if (x == null && y == null)
                return true;

            if (x == null && y != null)
                return false;

            if (x != null && y == null)
                return false;

            if (!string.Equals(x.SecurityClassification, y.SecurityClassification, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (x.Caveats != null && y.Caveats != null)
            {
                if (x.Caveats.Length != y.Caveats.Length)
                {
                    return false;
                }
                else if (x.Caveats.Length > 0 && y.Caveats.Length > 0)
                {
                    foreach (var caveat in x.Caveats)
                        if (Array.IndexOf(y.Caveats, caveat) == -1)
                            return false;
                }
            }

            if (!string.Equals(x.Expires, y.Expires, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (!string.Equals(x.DownTo, y.DownTo, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (x.InformationManagementMarkers != null && y.InformationManagementMarkers != null)
            {
                if (x.InformationManagementMarkers.Length != y.InformationManagementMarkers.Length)
                {
                    return false;
                }
                else if (x.InformationManagementMarkers.Length > 0 && y.InformationManagementMarkers.Length > 0)
                {
                    foreach (var marker in x.InformationManagementMarkers)
                        if (Array.IndexOf(y.InformationManagementMarkers, marker) == -1)
                            return false;
                }
            }

            if (!string.Equals(x.Dlm, y.Dlm, StringComparison.CurrentCultureIgnoreCase))
                return false;

            return true;
        }
    }
}
