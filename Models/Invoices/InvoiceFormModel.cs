using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class InvoiceFormModel
    {
        public int BookingID { get; set; }
        public Invoice Invoice { get; set; }
    }
}
