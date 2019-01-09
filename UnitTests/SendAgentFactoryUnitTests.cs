using AusGovLabellingAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SendAgentFactoryUnitTests
    {
        [TestMethod]
        public void CreateAgent()
        {
            Logging.Log.Debug("SendAgentFactoryUnitTests.CreateAgent()");

            var factory = new SendAgentFactory();
            Assert.IsNotNull(factory);

            var agent = factory.CreateAgent(null);
            Assert.IsNotNull(agent);
        }
    }
}
