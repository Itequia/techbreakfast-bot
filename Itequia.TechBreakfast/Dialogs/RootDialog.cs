using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Itequia.TechBreakfast.Data;
using Itequia.TechBreakfast.Data.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;

namespace Itequia.TechBreakfast.Dialogs
{
    [LuisModel("b72c0c3c-d9a4-4a3d-9711-65efbb257eb5", "9e4b7c45662243198e0319a74ec82449", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private const string EntityOrderNumberName = "orderNumber";
        private const string EntityProductName = "product";

        private Order _currentOrder;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (!await CheckForWelcome(context, message))
            {
                await context.PostAsync("Lo siento, no te he entendido. Si lo necesitas, puedes pedirme ayuda.");
                context.Done<object>(null);
            }
        }

        [LuisIntent("order.list")]
        public async Task ListOrders(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (!await CheckForWelcome(context, message))
            {
                var messageToForward = await message;
                await context.Forward(new ListOrdersDialog(), AfterDialog, messageToForward, CancellationToken.None);
            }

        }

        [LuisIntent("order.status")]
        public async Task OrderStatus(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (!await CheckForWelcome(context, message))
            {
                EntityRecommendation orderNumberRecommendation = null;
                if (result != null) result.TryFindEntity(EntityOrderNumberName, out orderNumberRecommendation);
                var msg = await message;

                var orderNum = result != null ? orderNumberRecommendation?.Entity : msg.Text;
                if (!string.IsNullOrWhiteSpace(orderNum))
                {
                    var resultString = Regex.Match(orderNum, @"\d+").Value;
                    bool isNumeric = int.TryParse(resultString, out var orderNumber);

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
        }


        [LuisIntent("order.create")]
        public async Task CreateOrder(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (!await CheckForWelcome(context, message))
            {
                EntityRecommendation productRecommendation = null;
                if (result != null) result.TryFindEntity(EntityProductName, out productRecommendation);
                var msg = await message;

                var product = result != null ? productRecommendation?.Entity : msg.Text;
                if (!string.IsNullOrWhiteSpace(product))
                {
                    if (!MemoryStorage.Products.Any(p => p.Name.ToLower().Contains(product.ToLower())))
                    {
                        await context.PostAsync($"Lo sentimos, el producto *{product}* no se encuentra en stock.");
                        context.Done<object>(null);
                    }
                    else
                    {
                        var selectedProduct =
                            MemoryStorage.Products.First(p => p.Name.ToLower().Contains(product.ToLower()));

                        PromptDialog.Confirm(
                            context,
                            AfterConfirmation,
                            $"**Confirmación pedido** \n\n" +
                            $"¿Desea comprar el producto *{selectedProduct.Name}* por un precio de **{selectedProduct.Price}€**?",
                            options: new[] { "Sí", "No" }, patterns: new string[][] { new[] { "Sí", "Confirmar", "Aceptar", "sí", "confirmar", "aceptar" }, new[] { "No", "Cancelar", "Atrás", "no", "cancelar", "atrás" } });

                        _currentOrder = new Order()
                        {
                            Product = selectedProduct.Name,
                            Date = DateTime.Now,
                            Price = selectedProduct.Price
                        };
                    }
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = "¿Qué te gustaría comprar? \n\n Tenemos los siguientes productos en stock:";
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = new List<Attachment>();

                    foreach (var stockProduct in MemoryStorage.Products)
                    {
                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButton = new CardAction()
                        {
                            Value = $"comprar {stockProduct.Name}",
                            Type = "postBack",
                            Title = "Comprar"
                        };

                        cardButtons.Add(plButton);

                        byte[] imageBytes = File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/" + stockProduct.Image));

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = stockProduct.Name,
                            Subtitle = stockProduct.Price + "€",
                            Text = stockProduct.Description,
                            Images = new List<CardImage>() { new CardImage() { Url = "data:image/png;base64," + Convert.ToBase64String(imageBytes) } },
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                    }

                    await context.PostAsync(reply, CancellationToken.None);
                    context.Done<object>(null);
                }
            }
            
        }

        [LuisIntent("help")]
        public async Task RequestHelp(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (!await CheckForWelcome(context, message))
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
        }

        private async Task<bool> CheckForWelcome(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var msg = await message;
            if (msg.Text.ToLower().Contains("hola") || msg.Text.ToLower().Contains("buenos días"))
            {
                await context.PostAsync("¡Bienvenido a nuestra tienda online! \U0001F604 \n\n ¿En qué puedo ayudarte?");
                context.Done<object>(null);
                return true;
            }

            return false;
        }
        private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }

        public async Task AfterConfirmation(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                if (_currentOrder != null)
                {
                    MemoryStorage.AddOrder(_currentOrder);

                    context.PostAsync(
                        $"**¡Pedido realizado! \U0001F44D** \n\n Muchas gracias por su compra. \n\n Su nuevo pedido ha sido asignado con el identificador **{ _currentOrder.Id }**. Si lo desea, puede consultar su estado.");
                }
            }
            else
            {
                context.PostAsync(
                    "Ningún problema, hemos cancelado su pedido.");
            }

            _currentOrder = null;
            context.Done<object>(null);
        }
    }
}
