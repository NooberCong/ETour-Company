using Core.Entities;
using Infrastructure.InterfaceImpls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class BlogListModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public IPost<Employee>.PostCategory Category { get; set; }
        public bool ShowHidden { get; set; }
    }
}
