using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Itequia.TechBreakfast.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Itequia.TechBreakfast.Dialogs
{
    [Serializable]
    public class CreateOrderDialog : IDialog<string>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
           
        }
    }
}