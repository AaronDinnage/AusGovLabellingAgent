using Microsoft.Exchange.Data.Transport.Smtp;

namespace AusGovLabellingAgent
{
    public class ReceiveAgent : SmtpReceiveAgent
    {
        public ReceiveAgent()
        {
            OnEndOfData += EndOfData;
        }

        private void EndOfData(ReceiveMessageEventSource source, EndOfDataEventArgs e)
        {
            Logging.Log.Information("ReceiveAgent.EndOfData() - Start");

            Common.ProcessInboundMessage(e.MailItem.Message);

            Logging.Log.Information("ReceiveAgent.EndOfData() - End");
        }
    }
}
