using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Routing;

namespace AusGovLabellingAgent
{
    public class SendAgent : RoutingAgent
    {
        private readonly SmtpServer _server;

        public SendAgent(SmtpServer server)
        {
            _server = server;
            OnSubmittedMessage += SubmittedMessage;
        }

        public void SubmittedMessage(SubmittedMessageEventSource source, QueuedMessageEventArgs e)
        {
            Logging.Log.Information("SendAgent.SubmittedMessage() - Start");

            Common.ProcessOutboundMessage(_server, source, e.MailItem.Message);

            Logging.Log.Information("SendAgent.SubmittedMessage() - End");
        }
    }
}
