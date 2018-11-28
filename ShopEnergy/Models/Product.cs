using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopEnergy.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        public string DescriptionText { get; set; }
        public string DescriptionImg { get; set; }
    }
}