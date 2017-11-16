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
            new Order()
            {
                Id = 1,
                Product = "Test product",
                Date =  DateTime.Now,
                Price = 25
            },
            new Order()
            {
                Id = 2,
                Product = "Test product 2",
                Date =  DateTime.Now,
                Price = 225
            }
        };

        public static void AddOrder(Order order)
        {
            order.Id = Orders.Any() ? Orders.Last().Id + 1 : 1;
            Orders.Add(order);
        }
    }
}