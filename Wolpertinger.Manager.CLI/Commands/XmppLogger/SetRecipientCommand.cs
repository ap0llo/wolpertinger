using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Set, "Recipient", "XmppLogger")]
    class SetRecipientCommand : LoggerCommand
    {
        [Parameter("Value", IsOptional = false, Position = 1)]
        public string Value { get; set; }

        public override void Execute()
        {
            var logger = getLoggerComponent();

            logger.SetRecipientAsync(Value);
        }
    }
}
