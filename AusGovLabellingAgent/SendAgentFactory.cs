using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Routing;

namespace AusGovLabellingAgent
{
    public sealed class SendAgentFactory : RoutingAgentFactory
    {
        public override RoutingAgent CreateAgent(SmtpServer server)
        {
            Logging.Log.Information("SendAgentFactory.CreateAgent()");

            return new SendAgent(server);
        }
    }
}
