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

        public static List<Product> Products = new List<Product>()
        {
            new Product() { Name = "Bicicleta", Image = "bici.png", Price = 129, Description = "Bicicleta simple y eficaz para iniciarse en el mundo del ciclismo."},
            new Product() { Name = "Palo de hockey", Image = "hockey.png", Price = 35, Description = "Stick de madera. Equipamiento oficial de la Federació Catalana de Hockey (FCH)."},
            new Product() { Name = "Raqueta de tenis", Image = "tenis.png", Price = 50, Description = "Ideal para jugadores principales o debutantes. Muy resistente a los golpes."},
            new Product() { Name = "Bate de béisbol", Image = "beisbol.png", Price = 45, Description = "Bate de espuma soft para iniciarse fácilmente al béisbol con total seguridad."},
            new Product() { Name = "Pelota de fútbol", Image = "futbol.png", Price = 20, Description = "Balón ideal para la práctica regular de fútbol 11." }
        };

        public static void AddOrder(Order order)
        {
            order.Id = Orders.Any() ? Orders.Last().Id + 1 : 1;
            Orders.Add(order);
        }
    }
}