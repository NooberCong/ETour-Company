using Core.Entities;
using Core.Interfaces;
using Infrastructure.InterfaceImpls;
using System.Collections.Generic;

namespace Company.Models
{
    public class AccountListModel
    {
        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<IEmployee> Employees { get; set; }
        public bool ShowBanned { get; set; }
    }
}
