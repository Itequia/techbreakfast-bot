using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Itequia.TechBreakfast.Data.Models;

namespace Itequia.TechBreakfast.Data
{
    public static class MemoryStorage
    {
        public static List<Order> Orders = new List<Order>()
        {
        };

        public static Dictionary<string, string> Products = new Dictionary<string, string>()
        {
            { "Bicicleta", "bici.png" },
            { "Palo de hockey", "hockey.png" },
            { "Raqueta de tenis", "tenis.png" },
            { "Bate de béisbol", "beisbol.png" },
            { "Pelota de fútbol", "futbol.png" }
        };

        public static void AddOrder(Order order)
        {
            order.Id = Orders.Any() ? Orders.Last().Id + 1 : 1;
            Orders.Add(order);
        }
    }
}