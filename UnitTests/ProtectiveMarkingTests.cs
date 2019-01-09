using AusGovLabellingAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass()]
    public class ProtectiveMarkingTests
    {
        [TestMethod()]
        public void SubjectTest()
        {
            Logging.Log.Debug("ProtectiveMarkingTests.SubjectTest()");

            ProtectiveMarking x;
            string output;

            // Official
            x = new ProtectiveMarking() { SecurityClassification = "OFFICIAL" };
            output = x.Subject();
            Assert.AreEqual(output, "[SEC=OFFICIAL]");

            // PROTECTED, Cabinet, Expires Election DownTo OFFICIAL
            x = new ProtectiveMarking()
            {
                SecurityClassification = "PROTECTED",
                Expires = "2019-01-01",
                DownTo = "OFFICIAL"
            };
            x.Caveats = new string[] { "RI:CABINET" };

            output = x.Subject();
            Assert.AreEqual(output, "[SEC=PROTECTED, CAVEAT=RI:CABINET, EXPIRES=2019-01-01, DOWNTO=OFFICIAL]");
        }

        [TestMethod()]
        public void HeaderTest()
        {
            Logging.Log.Debug("ProtectiveMarkingTests.HeaderTest()");

            ProtectiveMarking x;
            string header;

            // Official
            x = new ProtectiveMarking() { SecurityClassification = "OFFICIAL", Origin = "user@mail.com" };
            header = x.Header();
            Assert.AreEqual(header, "VER=2018.1, NS=gov.au, SEC=OFFICIAL, ORIGIN=user@mail.com");

            // PROTECTED, Cabinet, Expires Election DownTo OFFICIAL
            x = new ProtectiveMarking()
            {
                SecurityClassification = "PROTECTED",
                Expires = "Election",
                DownTo = "OFFICIAL",
                Origin = "user@mail.com"
            };
            x.Caveats = new string[] { "RI:CABINET" };
            header = x.Header();
            Assert.AreEqual(header, "VER=2018.1, NS=gov.au, SEC=PROTECTED, CAVEAT=RI:CABINET, EXPIRES=Election, DOWNTO=OFFICIAL, ORIGIN=user@mail.com");
        }

        [TestMethod()]
        public void FromSubjectTest()
        {
            Logging.Log.Debug("ProtectiveMarkingTests.FromSubjectTest()");

            ProtectiveMarking x;
            string input;
            string output;

            // Simple
            input = "[SEC=PROTECTED]";
            x = ProtectiveMarking.FromRegex(input, Config.Current.SubjectRegex, Config.Current.RegexOptions);
            output = x.Subject();
            Assert.AreEqual(input, output);

            // Complex
            input = "[SEC=PROTECTED, CAVEAT=RI:CABINET, EXPIRES=2019-01-01, DOWNTO=OFFICIAL]";
            x = ProtectiveMarking.FromRegex(input, Config.Current.SubjectRegex, Config.Current.RegexOptions);
            output = x.Subject();
            Assert.AreEqual(input, output);
        }

        [TestMethod()]
        public void FromHeaderTest()
        {
            Logging.Log.Debug("ProtectiveMarkingTests.FromHeaderTest()");

            ProtectiveMarking x;
            string input;
            string output;

            // Simple
            input = "VER=2018.1, NS=gov.au, SEC=PROTECTED";
            x = ProtectiveMarking.FromRegex(input, Config.Current.HeaderRegex, Config.Current.RegexOptions);
            output = x.Header();
            Assert.AreEqual(input, output);

            // Complex
            input = "VER=2018.1, NS=gov.au, SEC=PROTECTED, CAVEAT=RI:CABINET, EXPIRES=2019-01-01, DOWNTO=OFFICIAL";
            x = ProtectiveMarking.FromRegex(input, Config.Current.HeaderRegex, Config.Current.RegexOptions);
            output = x.Header();
            Assert.AreEqual(input, output);
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Logging.Log.Debug("ProtectiveMarkingTests.EqualsTest()");

            ProtectiveMarking x = null;
            ProtectiveMarking y = null;
            Assert.IsTrue(ProtectiveMarking.Equals(x, y));

            x = new ProtectiveMarking();
            y = new ProtectiveMarking();
            Assert.IsTrue(ProtectiveMarking.Equals(x, y));

            x = new ProtectiveMarking() { SecurityClassification = "Unofficial" };
            y = new ProtectiveMarking() { SecurityClassification = "UNOFFICIAL" };
            Assert.IsTrue(ProtectiveMarking.Equals(x, y));

            x = new ProtectiveMarking() { SecurityClassification = "Unclassified" };
            y = new ProtectiveMarking() { SecurityClassification = "Official" };
            Assert.IsFalse(ProtectiveMarking.Equals(x, y));
        }
    }
}