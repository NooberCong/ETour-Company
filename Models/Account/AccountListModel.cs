using Core.Entities;
using Infrastructure.InterfaceImpls;
using System.Collections.Generic;

namespace Company.Models
{
    public class AccountListModel
    {
        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
        public bool ShowBanned { get; set; }
    }
}
