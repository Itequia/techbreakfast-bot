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
    public class ListOrdersDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if (!MemoryStorage.Orders.Any())
                await context.PostAsync("Aún no has realizado ningún pedido");
            else
            {
                var reply = context.MakeMessage();
                reply.Text = "Estos son tus pedidos:";
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = new List<Attachment>();

                foreach (var order in MemoryStorage.Orders)
                {
                    List<CardAction> cardButtons = new List<CardAction>();

                    CardAction plButton = new CardAction()
                    {
                        Value = $"ver estado pedido número {order.Id}",
                        Type = "postBack",
                        Title = "Detalle pedido"
                    };

                    cardButtons.Add(plButton);

                    ReceiptCard plCard = new ReceiptCard()
                    {
                        Title = $"Pedido nº {order.Id}",
                        Items = new List<ReceiptItem>()
                        {
                            new ReceiptItem() { Subtitle = order.Product }
                        },
                        Total = order.Price.ToString() + "€",
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);
                }

                await context.PostAsync(reply, CancellationToken.None);
            }

            context.Done(string.Empty);
        }
    }
}