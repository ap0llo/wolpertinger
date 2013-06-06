using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Get, "Recipient", "XmppLogger")]
    class GetRecipientCommand : LoggerCommand
    {
        public override void Execute()
        {
            var logger = getLoggerComponent();

            string recipient = logger.GetRecipientAsync().Result.ToString();
            Context.WriteOutput(recipient.ToString());
        }
    }
}
