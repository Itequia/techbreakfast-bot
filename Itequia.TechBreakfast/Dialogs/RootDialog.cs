using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Itequia.TechBreakfast.Data;
using Itequia.TechBreakfast.Data.Models;
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
        private const string EntityProductName = "product";

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
            EntityRecommendation orderNumberRecommendation = null;
            if (result != null) result.TryFindEntity(EntityOrderNumberName, out orderNumberRecommendation);
            var msg = await message;

            var orderNum = result != null ? orderNumberRecommendation?.Entity : msg.Text;
            if (!string.IsNullOrWhiteSpace(orderNum))
            {
                bool isNumeric = int.TryParse(orderNum, out var orderNumber);

                if (isNumeric && MemoryStorage.Orders.Any(x => x.Id == orderNumber))
                {
                    Order order = MemoryStorage.Orders.First(x => x.Id == orderNumber);
                    await context.PostAsync($"Tu pedido número **{order.Id}** se realizó el día {order.Date:dd/MM/yyyy HH:mm}.\n\n Se encuentra en estado: ***pendiente de envío***.");
                    context.Done<object>(null);
                }
                else if (MemoryStorage.Orders.Any())
                {
                    await context.PostAsync("No tienes ningún pedido con ese número.");
                    await context.Forward(new ListOrdersDialog(), AfterDialog, msg, CancellationToken.None);
                }
                else
                {
                    await context.PostAsync("No tienes ningún pedido con ese número.");
                    context.Done<object>(null);
                }
            }
            else
            {
                await context.PostAsync("Introduce el número de pedido que deseas consultar");
                context.Wait((ctx, mg) => OrderStatus(ctx, mg, null));
            }
        }


        [LuisIntent("order.create")]
        public async Task CreateOrder(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            EntityRecommendation productRecommendation = null;
            if (result != null) result.TryFindEntity(EntityProductName, out productRecommendation);
            var msg = await message;

            var product = result != null ? productRecommendation?.Entity : msg.Text;
            if (!string.IsNullOrWhiteSpace(product))
            {
                if (MemoryStorage.Products.All(p =>
                    !string.Equals(p, product, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await context.PostAsync($"Lo sentimos, el producto *{ product }* no se encuentra en stock");
                    context.Done<object>(null);
                }
            }
            else
            {
                await context.PostAsync("¿Qué producto te gustaría comprar? \n\n Tenemos los siguientes en stock:");
                context.Wait((ctx, mg) => OrderStatus(ctx, mg, null));
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
    }
}
