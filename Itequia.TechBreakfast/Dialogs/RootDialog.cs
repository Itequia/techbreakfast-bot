using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Itequia.TechBreakfast.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Itequia.TechBreakfast.Dialogs
{
    [LuisModel("b72c0c3c-d9a4-4a3d-9711-65efbb257eb5", "9e4b7c45662243198e0319a74ec82449", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private const string EntityOrderNumberName = "orderNumber";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            await context.PostAsync("Lo siento, no te he entendido. Si lo necesitas, puedes pedirme ayuda.");
        }

        [LuisIntent("order.list")]
        public async Task ListOrders(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var messageToForward = await message;
            await context.Forward(new ListOrdersDialog(), AfterDialog, messageToForward, CancellationToken.None);
        }

        [LuisIntent("order.status")]
        public async Task OrderStatus(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            EntityRecommendation orderNumberRecommendation;

            if (result.TryFindEntity(EntityOrderNumberName, out orderNumberRecommendation))
            {

            }
            else
            {
                await context.PostAsync("¿Qué número de pedido deseas consultar?");
                context.Wait(OrderNumberReceived);
            }
        }

        [LuisIntent("help")]
        public async Task RequestHelp(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = "Puedo ayudarte con las siguientes tareas. ¿Qué deseas hacer?";
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction { Title = "Ver pedidos", Type = ActionTypes.PostBack, Value = "Ver pedidos" },
                    new CardAction { Title = "Consultar estado pedido", Type = ActionTypes.PostBack, Value = "Consultar estado pedido" },
                    new CardAction { Title = "Crear nuevo pedido", Type = ActionTypes.PostBack, Value = "Crear nuevo pedido" }
                }
            };

            await context.PostAsync(reply, CancellationToken.None);
            context.Done(string.Empty);
        }
        private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }

        private async Task OrderNumberReceived(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            int orderNumber;
            bool isNumeric = int.TryParse(activity.Text, out orderNumber);

            if (isNumeric && MemoryStorage.Orders.Any(x => x.Id == orderNumber))
            {
                await context.Forward(new OrderStatusDialog(), AfterDialog, activity, CancellationToken.None);
                return;
            }
            else if (MemoryStorage.Orders.Any())
            {
                await context.PostAsync("No tienes ningún pedido con ese número.");
                await context.PostAsync("Estos son tus pedidos:");
                await context.Forward(new ListOrdersDialog(), AfterDialog, activity, CancellationToken.None);
                return;
            }
            else 
            {
                await context.PostAsync("No tienes ningún pedido con ese número.");
                return;
            }
        }
    }
}
