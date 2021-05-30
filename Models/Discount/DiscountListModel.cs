using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class DiscountListModel
    {
        public IEnumerable<Discount> Discounts { get; set; }
        public bool ShowExpired { get; set; }
    }
}
