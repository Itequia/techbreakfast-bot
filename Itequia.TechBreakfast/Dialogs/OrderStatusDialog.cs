using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Itequia.TechBreakfast.Data;
using Itequia.TechBreakfast.Data.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Itequia.TechBreakfast.Dialogs
{
    [Serializable]
    public class OrderStatusDialog : IDialog<string>
    {
        private readonly string _entityOrderNumber;
        private bool _fromRootDialog;

        public OrderStatusDialog(string entityOrderNumber)
        {
            _entityOrderNumber = entityOrderNumber;
        }
        
        public async Task StartAsync(IDialogContext context)
        {
            _fromRootDialog = true;
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var orderNum = _fromRootDialog ? _entityOrderNumber : message.Text;
            if (!string.IsNullOrWhiteSpace(orderNum))
            {
                var activity = await result as Activity;
                bool isNumeric = int.TryParse(orderNum, out var orderNumber);

                if (isNumeric && MemoryStorage.Orders.Any(x => x.Id == orderNumber))
                {
                    Order order = MemoryStorage.Orders.First(x => x.Id == orderNumber);
                    await context.PostAsync($"Tu pedido número **{order.Id}** se realizó el día {order.Date:dd/MM/yyyy HH:mm}.\n\n Se encuentra en estado: ***pendiente de envío***.");
                }
                else if (MemoryStorage.Orders.Any())
                {
                    await context.PostAsync("No tienes ningún pedido con ese número.");
                    await context.Forward(new ListOrdersDialog(), AfterDialog, activity, CancellationToken.None);
                }
                else
                {
                    await context.PostAsync("No tienes ningún pedido con ese número.");
                }
                context.Done(string.Empty);
            }
            else
            {
                await context.PostAsync("Introduce el número de pedido que deseas consultar");
                _fromRootDialog = false;
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}