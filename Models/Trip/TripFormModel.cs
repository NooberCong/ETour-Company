using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TripFormModel
    {
        public string ReturnUrl { get; set; }
        public Trip Trip { get; set; }
        public Tour Tour { get; set; }
        public int[] AppliedDiscounts { get; set; } = new int[] { };
        public IEnumerable<Discount> Discounts { get; set; }
    }
}
