using AusGovLabellingAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ReceiveAgentFactoryUnitTests
    {
        [TestMethod]
        public void CreateAgent()
        {
            Logging.Log.Debug("ReceiveAgentFactoryUnitTests.CreateAgent()");

            var factory = new ReceiveAgentFactory();
            Assert.IsNotNull(factory);

            var agent = factory.CreateAgent(null);
            Assert.IsNotNull(agent);
        }
    }
}
