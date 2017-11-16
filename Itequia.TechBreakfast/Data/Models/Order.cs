using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Itequia.TechBreakfast.Data.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}