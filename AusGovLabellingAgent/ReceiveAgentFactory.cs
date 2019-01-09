using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Smtp;

namespace AusGovLabellingAgent
{
    public sealed class ReceiveAgentFactory : SmtpReceiveAgentFactory
    {
        public override SmtpReceiveAgent CreateAgent(SmtpServer server)
        {
            Logging.Log.Information("ReceiveAgentFactory.CreateAgent()");

            return new ReceiveAgent();
        }
    }
}
