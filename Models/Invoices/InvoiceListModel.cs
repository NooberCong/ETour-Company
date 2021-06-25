using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class InvoiceListModel
    {
        public IEnumerable<Invoice> Invoices { get; set; }
    }
}
