using Core.Interfaces;
using Infrastructure.InterfaceImpls;
using System.Collections.Generic;

namespace Company.Models
{
    public class EmployeeFormModel
    {
        public IEmployee Employee { get; set; }
        public IEnumerable<IRole> Roles { get; set; }
    }
}
