using Infrastructure.InterfaceImpls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models.Blog
{
    public class BlogListModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public bool ShowClosed { get; set; }
    }
}
